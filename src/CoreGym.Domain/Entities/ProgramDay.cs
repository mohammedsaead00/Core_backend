namespace CoreGym.Domain.Entities;

/// <summary>One training day inside a program template.</summary>
public class ProgramDay
{
    public Guid Id { get; set; }

    public Guid ProgramId { get; set; }

    public int DayNumber { get; set; }

    public string Name { get; set; } = null!;

    public string? NameAr { get; set; }

    /// <summary>Postgres text[] → JSON array of muscle-group names.</summary>
    public List<string>? MuscleGroups { get; set; }

    public string? Notes { get; set; }

    public TrainingProgram? Program { get; set; }
}
