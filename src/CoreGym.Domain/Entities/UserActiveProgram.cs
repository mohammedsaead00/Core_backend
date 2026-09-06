namespace CoreGym.Domain.Entities;

/// <summary>Pointer to the program template a user is currently following.</summary>
public class UserActiveProgram
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ProgramId { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public int? CurrentWeek { get; set; }

    public int? CurrentDay { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Profile? User { get; set; }

    public TrainingProgram? Program { get; set; }
}
