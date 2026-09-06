namespace CoreGym.Domain.Entities;

/// <summary>Content (file) published by a coach for clients.</summary>
public class CoachContent
{
    public Guid Id { get; set; }

    public Guid CoachId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Type { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public bool IsPublic { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public string? ThumbnailUrl { get; set; }

    public int? FileSizeKb { get; set; }

    public int SortOrder { get; set; }

    public Coach? Coach { get; set; }
}
