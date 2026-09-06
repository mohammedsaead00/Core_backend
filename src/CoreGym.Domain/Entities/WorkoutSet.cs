using CoreGym.Domain.Authorization;

namespace CoreGym.Domain.Entities;

/// <summary>One performed set, denormalized by exercise name (matches the original).</summary>
public class WorkoutSet : IOwnedResource
{
    public Guid Id { get; set; }

    /// <summary>Null for standalone sets not attached to a session.</summary>
    public Guid? SessionId { get; set; }

    public Guid UserId { get; set; }

    public string ExerciseName { get; set; } = null!;

    public int SetNumber { get; set; }

    public int? Reps { get; set; }

    public decimal? WeightKg { get; set; }

    public int? DurationSec { get; set; }

    public int? RestSec { get; set; }

    public bool IsWarmup { get; set; }

    public DateTimeOffset? LoggedAt { get; set; }

    public WorkoutSession? Session { get; set; }

    public Profile? User { get; set; }
}
