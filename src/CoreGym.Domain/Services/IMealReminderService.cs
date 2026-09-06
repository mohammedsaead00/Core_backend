namespace CoreGym.Domain.Services;

public record MealReminderRunSummary(
    int ProfilesChecked,
    int RemindersSent,
    int SkippedDisabled,
    int SkippedQuietHours,
    int SkippedAlreadyLogged,
    int SkippedAlreadyReminded);

/// <summary>
/// Replaces the send-meal-reminders Edge Function + coregym-meal-reminders
/// pg_cron job: for every profile, skip when reminders are disabled, when in
/// quiet hours (Cairo local time), when food was already logged today (Cairo),
/// or when this UTC window (06/12/18) already produced a reminder; otherwise
/// push and write the notification_log row.
/// </summary>
public interface IMealReminderService
{
    Task<MealReminderRunSummary> RunAsync(DateTimeOffset? nowUtc = null, CancellationToken cancellationToken = default);
}
