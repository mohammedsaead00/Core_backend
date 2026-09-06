namespace CoreGym.Domain.Entities;

/// <summary>Daily activity synced from the device (Health Connect). Not deletable by design.</summary>
public class DailyActivity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime? ActivityDate { get; set; }

    public int Steps { get; set; }

    public decimal ActiveCaloriesBurned { get; set; }

    public int? HeartRateAvg { get; set; }

    public int? ExerciseMinutes { get; set; }

    public string? Source { get; set; }

    public DateTimeOffset? SyncedAt { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? User { get; set; }
}
