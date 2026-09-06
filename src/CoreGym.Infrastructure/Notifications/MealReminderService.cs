using CoreGym.Domain.Entities;
using CoreGym.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Notifications;

public class MealReminderService : IMealReminderService
{
    public const string LogType = "meal_reminder";

    /// <summary>Wording INFERRED — the original push text is not in the inventory.</summary>
    public const string Title = "وقت الوجبة | Meal time";
    public const string Body = "لا تنسَ تسجيل وجباتك اليوم / Don't forget to log your meals today";

    // The original cron fired at 06:00/12:00/18:00 UTC (08:00/14:00/20:00 Cairo).
    private static readonly int[] WindowStartHoursUtc = [6, 12, 18];

    private readonly CoreGymDbContext _db;
    private readonly IPushNotificationService _push;

    public MealReminderService(CoreGymDbContext db, IPushNotificationService push)
    {
        _db = db;
        _push = push;
    }

    public async Task<MealReminderRunSummary> RunAsync(DateTimeOffset? nowUtc = null, CancellationToken cancellationToken = default)
    {
        var now = nowUtc ?? DateTimeOffset.UtcNow;
        var cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        var cairoNow = TimeZoneInfo.ConvertTime(now, cairo);
        var cairoDate = cairoNow.Date;

        var window = now.UtcDateTime.Hour switch
        {
            >= 6 and < 12 => 6,
            >= 12 and < 18 => 12,
            >= 18 => 18,
            _ => 0,
        };
        var summary = new MealReminderRunSummary(0, 0, 0, 0, 0, 0);
        if (window == 0)
        {
            return summary;
        }

        var windowStartUtc = now.UtcDateTime.Date.AddHours(window);

        var profileIds = await _db.Profiles
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in profileIds)
        {
            summary = summary with { ProfilesChecked = summary.ProfilesChecked + 1 };

            var preferences = await _db.NotificationPreferences
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);
            if (preferences?.MealRemindersEnabled == false)
            {
                summary = summary with { SkippedDisabled = summary.SkippedDisabled + 1 };
                continue;
            }

            if (preferences?.QuietHoursStart is { } quietStart && preferences.QuietHoursEnd is { } quietEnd
                && IsInQuietHours(TimeOnly.FromTimeSpan(cairoNow.TimeOfDay), TimeOnly.FromTimeSpan(quietStart), TimeOnly.FromTimeSpan(quietEnd)))
            {
                summary = summary with { SkippedQuietHours = summary.SkippedQuietHours + 1 };
                continue;
            }

            var loggedToday = await _db.NutritionLogs
                .AnyAsync(n => n.UserId == userId && n.LoggedDate == cairoDate, cancellationToken);
            if (loggedToday)
            {
                summary = summary with { SkippedAlreadyLogged = summary.SkippedAlreadyLogged + 1 };
                continue;
            }

            var alreadyReminded = await _db.NotificationLogs
                .AnyAsync(l => l.UserId == userId && l.Type == LogType && l.SentAt >= windowStartUtc, cancellationToken);
            if (alreadyReminded)
            {
                summary = summary with { SkippedAlreadyReminded = summary.SkippedAlreadyReminded + 1 };
                continue;
            }

            try
            {
                await _push.SendToUsersAsync(new[] { userId }, Title, Body, null, cancellationToken);
            }
            catch (Exception)
            {
                // Push failure: don't log the reminder so the next probe in this
                // window retries the user.
                continue;
            }

            _db.NotificationLogs.Add(new NotificationLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = LogType,
                Title = Title,
                Body = Body,
                Data = $$"""{"window":"{{windowStartUtc:HH}}"}""",
                SentAt = now,
            });
            summary = summary with { RemindersSent = summary.RemindersSent + 1 };
        }

        await _db.SaveChangesAsync(cancellationToken);
        return summary;
    }

    private static bool IsInQuietHours(TimeOnly current, TimeOnly start, TimeOnly end) =>
        start <= end
            ? current >= start && current < end
            : current >= start || current < end; // overnight span (e.g. 22:00 → 06:30)
}
