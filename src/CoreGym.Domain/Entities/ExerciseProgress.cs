using CoreGym.Domain.Authorization;

namespace CoreGym.Domain.Entities;

/// <summary>Per-session computed progress for an exercise (feeds personal records).</summary>
public class ExerciseProgress : IOwnedResource
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? ExerciseId { get; set; }

    public DateTime? SessionDate { get; set; }

    public decimal? BestSetWeight { get; set; }

    public int? BestSetReps { get; set; }

    public decimal? TotalVolume { get; set; }

    public decimal? OneRmEstimate { get; set; }

    public Guid? SessionId { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? User { get; set; }

    public Exercise? Exercise { get; set; }

    public WorkoutSession? Session { get; set; }
}
