namespace CoreGym.Domain.Entities;

/// <summary>Structured training program template; public read.</summary>
public class TrainingProgram
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? NameAr { get; set; }

    public string? Description { get; set; }

    public string? DescriptionAr { get; set; }

    public string? Level { get; set; }

    public string? Goal { get; set; }

    public int? DaysPerWeek { get; set; }

    public int? DurationWeeks { get; set; }

    public string? SplitType { get; set; }

    public bool? IsActive { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
}
