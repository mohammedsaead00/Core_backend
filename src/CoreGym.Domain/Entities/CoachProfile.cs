namespace CoreGym.Domain.Entities;

/// <summary>
/// Public coach profile. Primary key IS the auth user id (per the original schema),
/// unlike Coach whose id is separate from its user_id.
/// </summary>
public class CoachProfile
{
    public Guid Id { get; set; }

    public string? Bio { get; set; }

    public string? BioAr { get; set; }

    public List<string>? Specialties { get; set; }

    public List<string>? Certifications { get; set; }

    public int ExperienceYears { get; set; }

    public decimal PricePerMonth { get; set; }

    public string? Currency { get; set; }

    public decimal Rating { get; set; }

    public int ReviewsCount { get; set; }

    public bool? IsAvailable { get; set; }

    public bool IsVerified { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? InstagramUrl { get; set; }

    public string? YoutubeUrl { get; set; }

    public int? MaxClients { get; set; }

    public int CurrentClients { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Profile? User { get; set; }
}
