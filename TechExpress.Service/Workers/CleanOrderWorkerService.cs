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
                var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
                var strategy = unitOfWork.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    var cutoff = DateTimeOffset.Now - ExpireAfter;

                    // Lấy danh sách orders cần xóa (có kèm Items)
                    var expiredOrders = await unitOfWork.OrderRepository
                        .FindExpiredUnpaidOrdersWithItemsAsync(cutoff, ct);

                    if (!expiredOrders.Any())
                        return;

                    using var transaction = await unitOfWork.BeginTransactionAsync();
                    try
                    {
                        // Restock lại sản phẩm cho từng order
                        foreach (var order in expiredOrders)
                        {
                            foreach (var item in order.Items)
                            {
                                await unitOfWork.ProductRepository
                                    .IncrementStockAtomicAsync(item.ProductId, item.Quantity);
                            }
                        }

                        // Xóa các orders đã hết hạn theo danh sách IDs đã lấy
                        var orderIds = expiredOrders.Select(o => o.Id).ToList();
                        var deleted = await unitOfWork.OrderRepository
                            .DeleteOrdersByIdsAsync(orderIds, ct);

                        await unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();

                        if (deleted > 0)
                        {
                            _logger.LogInformation(
                                "CleanOrderWorkerService deleted {Count} expired unpaid pending orders and restocked products (cutoff: {Cutoff}).",
                                deleted, cutoff);
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
