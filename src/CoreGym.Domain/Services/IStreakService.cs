namespace CoreGym.Domain.Services;

/// <summary>
/// Replaces the record_daily_activity(p_source) RPC: logs today's activity for
/// the given source ('workout' or 'nutrition') and updates the streak —
/// increments on consecutive days, applies the once-a-month freeze to forgive
/// a single missed day, resets to 1 otherwise. Idempotent per source per day.
/// SEMANTICS INFERRED from the inventory description — validate against
/// prod via pg_get_functiondef('record_daily_activity').
/// </summary>
public interface IStreakService
{
    Task RecordDailyActivityAsync(Guid userId, string source, DateTime? activityDate = null, CancellationToken cancellationToken = default);

    /// <summary>Replaces the get_streak_status() RPC; null when the user has no streak row yet.</summary>
    Task<ReadModels.StreakStatus?> GetStreakStatusAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Replaces the streak-freeze-monthly-reset pg_cron job (call from a scheduler).</summary>
    Task<int> ResetMonthlyFreezesAsync(CancellationToken cancellationToken = default);
}
