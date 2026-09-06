using CoreGym.Domain.Enums;

namespace CoreGym.Domain.Entities;

/// <summary>One row per app user; id = Supabase auth user id.</summary>
public class Profile
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Gender { get; set; }

    public int? Age { get; set; }

    public decimal? WeightKg { get; set; }

    public decimal? HeightCm { get; set; }

    public string? FitnessGoal { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public UserRole Role { get; set; } = UserRole.Client;

    public string? FullName { get; set; }
}
