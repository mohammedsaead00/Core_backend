namespace CoreGym.Domain.Entities;

/// <summary>An exercise slotted into a program day, with ordering and prescription.</summary>
public class ProgramDayExercise
{
    public Guid Id { get; set; }

    public Guid ProgramDayId { get; set; }

    public Guid? ExerciseId { get; set; }

    public int OrderIndex { get; set; }

    public int? Sets { get; set; }

    public int? RepsMin { get; set; }

    public int? RepsMax { get; set; }

    public int? RestSeconds { get; set; }

    public string? Notes { get; set; }

    public string? NotesAr { get; set; }

    public bool IsMainLift { get; set; }

    public ProgramDay? ProgramDay { get; set; }

    public Exercise? Exercise { get; set; }
}
