using CoreGym.Domain.Authorization;

namespace CoreGym.Domain.Entities;

/// <summary>
/// Rolled-up per-day totals for a user (nutrition/workout/activity targets and
/// actuals). Fed by the sync triggers in a later phase.
/// </summary>
public class DailySummary : IOwnedResource
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime? SummaryDate { get; set; }

    public int? Steps { get; set; }

    public int? ActiveMinutes { get; set; }

    public int? CaloriesBurned { get; set; }

    public int? WaterMl { get; set; }

    public decimal? SleepHours { get; set; }

    public decimal? CaloriesConsumed { get; set; }

    public decimal? ProteinG { get; set; }

    public decimal? CarbsG { get; set; }

    public decimal? FatG { get; set; }

    public bool? WorkoutDone { get; set; }

    public int? WorkoutDuration { get; set; }

    public int? Mood { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Profile? User { get; set; }
}
