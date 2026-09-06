namespace CoreGym.Domain.ReadModels;

/// <summary>
/// Per-user weekly goal-completion rollup. Backs the weekly_progress view.
/// DEFINITION INFERRED (2026-09-06) — validate against prod pg_get_viewdef.
/// </summary>
public class WeeklyProgress
{
    public Guid UserId { get; set; }

    public DateTime WeekStart { get; set; }

    public int DaysLogged { get; set; }

    public int DaysGoalMet { get; set; }

    public decimal? AvgActualPct { get; set; }

    public decimal? AvgGoalPct { get; set; }
}
