namespace CoreGym.Domain.ReadModels;

/// <summary>Result shape of the get_streak_status() RPC replacement.</summary>
public class StreakStatus
{
    public Guid UserId { get; set; }

    public int CurrentStreak { get; set; }

    public int LongestStreak { get; set; }

    public DateTime? LastActiveDate { get; set; }

    public bool LoggedWorkoutToday { get; set; }

    public bool LoggedNutritionToday { get; set; }

    /// <summary>Streak is alive but nothing logged today yet.</summary>
    public bool AtRisk { get; set; }

    public int FreezeAvailable { get; set; }
}
