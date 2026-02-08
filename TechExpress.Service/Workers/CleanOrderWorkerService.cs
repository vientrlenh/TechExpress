using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TechExpress.Repository.Contexts;
using TechExpress.Repository.Repositories;

namespace TechExpress.Service.Workers
{
    public class CleanOrderWorkerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CleanOrderWorkerService> _logger;

        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan ExpireAfter = TimeSpan.FromMinutes(15);

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
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var repo = new OrderRepository(db);

                var cutoff = DateTimeOffset.Now - ExpireAfter;

                var deleted = await repo.DeleteExpiredUnpaidOrdersAsync(cutoff, ct);

                if (deleted > 0)
                {
                    _logger.LogInformation(
                        "CleanOrderWorkerService deleted {Count} expired unpaid pending orders (cutoff: {Cutoff}).",
                        deleted, cutoff);
                }
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
