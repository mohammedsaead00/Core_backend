namespace CoreGym.Domain.Entities;

/// <summary>
/// One row per user (PK = user_id). Written only by the streak logic
/// (replaces record_daily_activity()/get_streak_status() in a later phase).
/// </summary>
public class UserStreak
{
    public Guid UserId { get; set; }

    public int CurrentStreak { get; set; }

    public int LongestStreak { get; set; }

    public DateTime? LastActiveDate { get; set; }

    public int? FreezeAvailable { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Profile? User { get; set; }
}
