using CoreGym.Domain.Authorization;

namespace CoreGym.Domain.Entities;

/// <summary>A user's workout session.</summary>
public class WorkoutSession : IOwnedResource
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string MuscleGroup { get; set; } = null!;

    public string? SessionName { get; set; }

    public int DurationMin { get; set; }

    public string? Notes { get; set; }

    public DateTime? SessionDate { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public Profile? User { get; set; }
}
