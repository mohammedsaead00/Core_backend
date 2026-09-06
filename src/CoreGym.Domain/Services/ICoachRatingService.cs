namespace CoreGym.Domain.Services;

/// <summary>
/// Replaces the update_coach_rating / refresh_coach_rating triggers: recomputes
/// coaches.rating and coach_profiles.rating (plus reviews_count) as the average
/// of the coach's reviews. Call after every review insert/update/delete.
/// </summary>
public interface ICoachRatingService
{
    Task RecalculateCoachRatingAsync(Guid coachId, CancellationToken cancellationToken = default);
}
