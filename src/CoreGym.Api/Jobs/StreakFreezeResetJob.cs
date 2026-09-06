using CoreGym.Domain.Services;
using CoreGym.Infrastructure;

namespace CoreGym.Api.Jobs;

/// <summary>
/// Replaces the streak-freeze-monthly-reset pg_cron job: restores
/// freeze_available = 1 on the 1st of each month (UTC). The reset itself is
/// idempotent, so the hourly probe running multiple times on the 1st is safe.
/// </summary>
public class StreakFreezeResetJob(IServiceScopeFactory scopeFactory, ILogger<StreakFreezeResetJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (DateTime.UtcNow.Day == 1)
                {
                    using var scope = scopeFactory.CreateScope();
                    var streaks = scope.ServiceProvider.GetRequiredService<IStreakService>();
                    var reset = await streaks.ResetMonthlyFreezesAsync(stoppingToken);
                    if (reset > 0)
                    {
                        logger.LogInformation("Streak freeze monthly reset applied to {Count} users", reset);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Streak freeze monthly reset failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
