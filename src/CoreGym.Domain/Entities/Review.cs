namespace CoreGym.Domain.Entities;

/// <summary>
/// Canonical review table (decision 2026-09-06): the app reads `reviews` only;
/// `coach_reviews` is NOT ported. Aggregation into coaches.rating /
/// coach_profiles.rating (update_coach_rating trigger) is deferred to the
/// business-logic phase.
/// </summary>
public class Review
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }

    public Guid CoachId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? Client { get; set; }

    public Coach? Coach { get; set; }
}
