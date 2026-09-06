using CoreGym.Domain.Entities;
using CoreGym.Domain.ReadModels;
using CoreGym.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Services;

public class StreakService : IStreakService
{
    public const string SourceWorkout = "workout";
    public const string SourceNutrition = "nutrition";

    private readonly CoreGymDbContext _db;

    public StreakService(CoreGymDbContext db)
    {
        _db = db;
    }

    public async Task RecordDailyActivityAsync(Guid userId, string source, DateTime? activityDate = null, CancellationToken cancellationToken = default)
    {
        var normalizedSource = source?.Trim().ToLowerInvariant()
            ?? throw new ArgumentException("Activity source is required.", nameof(source));
        if (normalizedSource is not (SourceWorkout or SourceNutrition))
        {
            throw new ArgumentException(
                $"Activity source must be '{SourceWorkout}' or '{SourceNutrition}'.", nameof(source));
        }

        var today = (activityDate ?? DateTime.UtcNow).Date;

        // Append-only activity feed; one row per user/date/source.
        var alreadyLogged = await _db.StreakActivityLogs
            .AnyAsync(l => l.UserId == userId && l.ActivityDate == today && l.Source == normalizedSource, cancellationToken);
        if (!alreadyLogged)
        {
            _db.StreakActivityLogs.Add(new StreakActivityLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ActivityDate = today,
                Source = normalizedSource,
            });
        }

        var streak = await _db.UserStreaks.SingleOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (streak is null)
        {
            streak = new UserStreak { UserId = userId };
            _db.UserStreaks.Add(streak);
        }

        if (streak.LastActiveDate != today)
        {
            if (streak.LastActiveDate == today.AddDays(-1))
            {
                streak.CurrentStreak++;
            }
            else if (streak.LastActiveDate == today.AddDays(-2) && (streak.FreezeAvailable ?? 0) >= 1)
            {
                // Exactly one missed day: the once-a-month freeze forgives it.
                streak.FreezeAvailable = streak.FreezeAvailable!.Value - 1;
                streak.CurrentStreak++;
            }
            else
            {
                streak.CurrentStreak = 1;
            }

            if (streak.CurrentStreak > streak.LongestStreak)
            {
                streak.LongestStreak = streak.CurrentStreak;
            }

            streak.LastActiveDate = today;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<StreakStatus?> GetStreakStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var streak = await _db.UserStreaks
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (streak is null)
        {
            return null;
        }

        var today = DateTime.UtcNow.Date;
        var loggedWorkout = await _db.StreakActivityLogs
            .AnyAsync(l => l.UserId == userId && l.ActivityDate == today && l.Source == SourceWorkout, cancellationToken);
        var loggedNutrition = await _db.StreakActivityLogs
            .AnyAsync(l => l.UserId == userId && l.ActivityDate == today && l.Source == SourceNutrition, cancellationToken);

        return new StreakStatus
        {
            UserId = userId,
            CurrentStreak = streak.CurrentStreak,
            LongestStreak = streak.LongestStreak,
            LastActiveDate = streak.LastActiveDate,
            LoggedWorkoutToday = loggedWorkout,
            LoggedNutritionToday = loggedNutrition,
            AtRisk = streak.CurrentStreak > 0 && !loggedWorkout && !loggedNutrition,
            FreezeAvailable = streak.FreezeAvailable ?? 0,
        };
    }

    public async Task<int> ResetMonthlyFreezesAsync(CancellationToken cancellationToken = default)
    {
        // The pg_cron job resets freeze_available = 1 "for anyone below it" —
        // done via SaveChanges (not ExecuteUpdate) because user_streaks carries
        // the updated_at trigger.
        var stale = await _db.UserStreaks
            .Where(s => s.FreezeAvailable < 1)
            .ToListAsync(cancellationToken);

        foreach (var streak in stale)
        {
            streak.FreezeAvailable = 1;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return stale.Count;
    }
}
