namespace CoreGym.Domain.Entities;

/// <summary>A user's custom (non-template) program entry.</summary>
public class UserProgram
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string ProgramName { get; set; } = null!;

    public string MuscleGroup { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public Profile? User { get; set; }
}
