using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechExpress.Repository;

namespace TechExpress.Service.Workers;

public class ChangePromotionStatusWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ChangePromotionStatusWorker> _logger;

    public ChangePromotionStatusWorker(IServiceScopeFactory serviceScopeFactory, ILogger<ChangePromotionStatusWorker> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Change promotion status worker is starting...");
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

            var strategy = unitOfWork.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    var now = DateTimeOffset.Now;
                    var startAndEndPromotions = await unitOfWork.PromotionRepository.FindAllStartAndEndPromotionsWithTrackingAsync(now);

                    var startPromotions = startAndEndPromotions.Where(p => p.StartDate <= now && p.EndDate > now && !p.IsActive).ToList();
                    var startPromotionCount = startPromotions.Count;

                    var endPromotions = startAndEndPromotions.Where(p => p.EndDate < now && p.IsActive).ToList();
                    var endPromotionCount = endPromotions.Count;

                    foreach (var start in startPromotions)
                    {
                        start.IsActive = true;
                        start.UpdatedAt = DateTimeOffset.Now;
                    }
                    foreach (var end in endPromotions)
                    {
                        end.IsActive = false;
                        end.UpdatedAt = DateTimeOffset.Now;
                    }
                    await unitOfWork.SaveChangesAsync();
                    if (startPromotionCount > 0)
                    {
                        _logger.LogInformation("Change promotion status worker has enabled {Count} promotion(s)", startPromotionCount);
                    }
                    if (endPromotionCount > 0)
                    {
                        _logger.LogInformation("Change promotion status worker has disabled {Count} promotion(s)", endPromotionCount);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Change promotion status worker encounter an error: {}", ex.Message);
            }
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
