namespace CoreGym.Domain.Entities;

/// <summary>
/// Append-only activity feed that drives streak calculation. Written only by
/// the streak logic (record_daily_activity replacement, later phase); RLS in
/// the original allows SELECT only.
/// </summary>
public class StreakActivityLog
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime ActivityDate { get; set; }

    /// <summary>'workout' or 'nutrition' — enforced by a CHECK constraint.</summary>
    public string Source { get; set; } = null!;

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? User { get; set; }
}
