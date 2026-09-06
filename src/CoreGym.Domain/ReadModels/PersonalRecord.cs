namespace CoreGym.Domain.ReadModels;

/// <summary>
/// One row per (user, exercise): the user's personal records. Backs the
/// personal_records view. DEFINITION INFERRED (2026-09-06) — the inventory
/// lists the view but not its SQL; validate against prod pg_get_viewdef.
/// </summary>
public class PersonalRecord
{
    public Guid UserId { get; set; }

    public string ExerciseName { get; set; } = null!;

    public decimal? BestWeightKg { get; set; }

    public int? BestReps { get; set; }

    public decimal? BestSetVolume { get; set; }

    public decimal? EstimatedOneRm { get; set; }

    public DateTimeOffset? LastLoggedAt { get; set; }
}
