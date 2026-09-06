namespace CoreGym.Domain.Entities;

/// <summary>User's daily nutrition/activity targets.</summary>
public class UserGoal
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int? DailyCalories { get; set; }

    public int? DailyProteinG { get; set; }

    public int? DailyCarbsG { get; set; }

    public int? DailyFatG { get; set; }

    public int? DailyWaterMl { get; set; }

    public int? DailySteps { get; set; }

    public decimal? DailySleepHours { get; set; }

    public int? WeeklyWorkouts { get; set; }

    public decimal? TargetWeightKg { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Profile? User { get; set; }
}
