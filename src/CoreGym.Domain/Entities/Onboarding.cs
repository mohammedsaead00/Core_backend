namespace CoreGym.Domain.Entities;

/// <summary>Client onboarding answers; one row per user flow.</summary>
public class Onboarding
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int? Age { get; set; }

    public string? Gender { get; set; }

    public decimal? HeightCm { get; set; }

    public decimal? WeightKg { get; set; }

    public string? Goal { get; set; }

    public string? ActivityLevel { get; set; }

    public decimal? TargetWeight { get; set; }

    public int? WeeklyWorkouts { get; set; }

    public bool Completed { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Profile? User { get; set; }
}
