namespace CoreGym.Domain.ReadModels;

/// <summary>
/// Per-user weight history with day-over-day change. Backs the weight_progress
/// view. DEFINITION INFERRED (2026-09-06) — validate against prod pg_get_viewdef.
/// </summary>
public class WeightProgress
{
    public Guid UserId { get; set; }

    public DateTime? MeasuredDate { get; set; }

    public decimal? WeightKg { get; set; }

    public decimal? WeightChangeKg { get; set; }
}
