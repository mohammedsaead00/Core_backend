namespace CoreGym.Domain.Entities;

/// <summary>Weekly goal-completion percentage per day (drives the weekly chart).</summary>
public class WeeklyActivity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime WeekStart { get; set; }

    public int DayIndex { get; set; }

    public decimal? ActualPct { get; set; }

    public decimal GoalPct { get; set; }

    public Profile? User { get; set; }
}
