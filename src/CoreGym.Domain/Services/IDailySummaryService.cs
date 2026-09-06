namespace CoreGym.Domain.Services;

/// <summary>
/// Replaces the sync_nutrition_to_summary / sync_workout_to_summary triggers:
/// rolls the day's nutrition totals and workout state into daily_summary.
/// </summary>
public interface IDailySummaryService
{
    /// <summary>Recomputes calories/macros for the user's day from nutrition_logs (upsert).</summary>
    Task SyncNutritionAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>Recomputes workout_done / workout_duration for the user's day from workout_sessions (upsert).</summary>
    Task SyncWorkoutAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default);
}
