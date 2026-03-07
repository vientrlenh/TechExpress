using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TechExpress.Repository;
using TechExpress.Repository.Contexts;

namespace TechExpress.Service.Workers
{
    public class CleanOrderWorkerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CleanOrderWorkerService> _logger;

        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

        public CleanOrderWorkerService(
            IServiceScopeFactory scopeFactory,
            ILogger<CleanOrderWorkerService> logger)
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
                    var now = DateTimeOffset.Now;
                    var expiration = now.AddMinutes(-15);
                    await using var transaction = await unitOfWork.BeginTransactionAsync();
                    try
                    {
                        await unitOfWork.PromotionRepository.ReclaimUserUsageOnExpiredOrders(expiration);
                        await unitOfWork.PromotionUsageRepository.DeletePromotionUsagesOnExpiredOrders(expiration);
                        await unitOfWork.ProductRepository.RestockProductAfterPendingOrderExpiration(expiration);
                        var deleted = await unitOfWork.OrderRepository.DeleteExpiredUnpaidOrdersAsync(expiration);

                        await unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();

                        if (deleted > 0)
                        {
                            _logger.LogInformation(
                                "CleanOrderWorkerService deleted {Count} expired unpaid pending orders and restocked products (cutoff: {Cutoff}).",
                                deleted, expiration);
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
                _logger.LogError(ex, "CleanOrderWorkerService error while deleting expired unpaid orders.");
            }
        }
    }
}
