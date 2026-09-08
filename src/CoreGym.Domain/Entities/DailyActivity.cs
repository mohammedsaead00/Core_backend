namespace CoreGym.Domain.Entities;

/// <summary>Daily activity synced from the device (Health Connect). Not deletable by design.</summary>
public class DailyActivity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime? ActivityDate { get; set; }

    public int Steps { get; set; }

    public decimal ActiveCaloriesBurned { get; set; }

    /// <summary>Live prod dump: numeric, not integer.</summary>
    public decimal? HeartRateAvg { get; set; }

    public decimal? ExerciseMinutes { get; set; }

    public string? Source { get; set; }

    public DateTimeOffset? SyncedAt { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? User { get; set; }
}
