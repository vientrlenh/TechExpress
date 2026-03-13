using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using TechExpress.Repository;

namespace TechExpress.Service.Workers
{
    public class SetOrderCompleteWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SetOrderCompleteWorker> _logger;

        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

        public SetOrderCompleteWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<SetOrderCompleteWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(CheckInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunOnce(stoppingToken);
                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }

        private async Task RunOnce(CancellationToken ct)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
                var strategy = unitOfWork.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    var deliveredCutoff = DateTimeOffset.Now.AddDays(-3);
                    await using var transaction = await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);
                    try
                    {
                        var autoCompleted = await unitOfWork.OrderRepository.AutoCompleteDeliveredOrdersAsync(deliveredCutoff);

                        await transaction.CommitAsync();

                        if (autoCompleted > 0)
                        {
                            _logger.LogInformation(
                                "SetOrderCompleteWorker auto-completed {Count} delivered/picked-up orders after 3 days (cutoff: {Cutoff}).",
                                autoCompleted,
                                deliveredCutoff);
                        }
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetOrderCompleteWorker error while auto-completing orders.");
            }
        }
    }
}
