using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechExpress.Repository;
using TechExpress.Repository.Enums;

namespace TechExpress.Service.Workers;

public class CompleteOrderWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CompleteOrderWorker> _logger;

    public CompleteOrderWorker(IServiceScopeFactory serviceScopeFactory, ILogger<CompleteOrderWorker> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Complete order worker is starting...");
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
            var strategy = unitOfWork.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    var autoCompletedTime = DateTimeOffset.Now.AddDays(-3);

                    var uncompletedOrders = await unitOfWork.OrderRepository.FindPickedUpOrDeliveredReachAutoCompletedTimeAsync(autoCompletedTime);
                    var uncompletedOrderCount = uncompletedOrders.Count;
                    foreach (var order in uncompletedOrders)
                    {
                        if (order.PaidType is PaidType.Installment)
                        {
                            order.Status = OrderStatus.Installing;
                        }
                        else
                        {
                            order.Status = OrderStatus.Completed;
                        }
                    }
                    await unitOfWork.SaveChangesAsync();
                    if (uncompletedOrderCount > 0)
                    {
                        _logger.LogInformation("Complete order worker has auto completed {Count} order(s)", uncompletedOrderCount);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Complete order worker encounter an error: {}", ex.Message);
            }
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
        

    }
}
