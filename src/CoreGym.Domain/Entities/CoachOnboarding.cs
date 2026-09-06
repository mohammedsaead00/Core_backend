namespace CoreGym.Domain.Entities;

/// <summary>Coach signup/onboarding submission.</summary>
public class CoachOnboarding
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? DisplayName { get; set; }

    public int YearsExperience { get; set; }

    public List<string>? Certifications { get; set; }

    public List<string>? Specialization { get; set; }

    public string? Bio { get; set; }

    public decimal? PriceMonthly { get; set; }

    public decimal? PricePremium { get; set; }

    public List<string>? Languages { get; set; }

    public int? MaxClients { get; set; }

    public string? ProfileImageUrl { get; set; }

    public string? IntroVideoUrl { get; set; }

    public bool IsCompleted { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string? PhoneNumber { get; set; }

    public string? City { get; set; }

    public string? Gender { get; set; }

    public List<string>? GalleryImages { get; set; }

    public List<string>? PdfUrls { get; set; }

    public List<string>? CertificateFiles { get; set; }

    public List<string>? TransformationImages { get; set; }

    public Profile? User { get; set; }
}
