namespace CoreGym.Domain.Entities;

/// <summary>Exercise reference data; public read.</summary>
public class Exercise
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? NameAr { get; set; }

    public string? MuscleGroup { get; set; }

    /// <summary>Postgres text[] → JSON array of muscle names.</summary>
    public List<string>? SecondaryMuscles { get; set; }

    public string? Equipment { get; set; }

    public string? Category { get; set; }

    public string? Instructions { get; set; }

    public string? InstructionsAr { get; set; }

    public string? Tips { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public string? ImageUrl { get; set; }

    public string? YoutubeVideoId { get; set; }

    public string? GifUrl { get; set; }
}
