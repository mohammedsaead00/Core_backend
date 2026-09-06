using CoreGym.Domain.Services;

namespace CoreGym.Api.Jobs;

/// <summary>
/// Replaces the coregym-meal-reminders pg_cron job: probes every 10 minutes and
/// runs the reminder pass when the UTC hour is 06, 12 or 18 (the original cron
/// schedule). De-duplicates each window in memory; the reminder pass itself is
/// also idempotent via notification_log.
/// </summary>
public class MealReminderJob(IServiceScopeFactory scopeFactory, ILogger<MealReminderJob> logger) : BackgroundService
{
    private readonly HashSet<string> _completedWindows = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunIfDueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Meal reminder pass failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunIfDueAsync(CancellationToken stoppingToken)
    {
        var now = DateTime.UtcNow;
        var windowStartHour = now.Hour switch
        {
            >= 6 and < 12 => 6,
            >= 12 and < 18 => 12,
            >= 18 => 18,
            _ => 0,
        };

        if (windowStartHour == 0)
        {
            return;
        }

        var windowKey = $"{now:yyyy-MM-dd}-{windowStartHour}";
        if (!_completedWindows.Add(windowKey))
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var reminders = scope.ServiceProvider.GetRequiredService<IMealReminderService>();
        var summary = await reminders.RunAsync(cancellationToken: stoppingToken);
        logger.LogInformation(
            "Meal reminders: {Sent} sent, {Checked} profiles ({Disabled} disabled, {Quiet} quiet hours, {Logged} already logged, {Reminded} already reminded)",
            summary.RemindersSent, summary.ProfilesChecked, summary.SkippedDisabled,
            summary.SkippedQuietHours, summary.SkippedAlreadyLogged, summary.SkippedAlreadyReminded);
    }
}
