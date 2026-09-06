namespace CoreGym.Domain.Entities;

/// <summary>An AI food-photo scan (analyze-food function output).</summary>
public class FoodScan
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? ImagePath { get; set; }

    public bool? IsFood { get; set; }

    public string? Confidence { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset? ScannedAt { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? User { get; set; }
}
