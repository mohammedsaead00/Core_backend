namespace CoreGym.Domain.Entities;

/// <summary>
/// A coach's subscription plan. NOTE (from the original RLS): coach_id here is
/// the AUTH USER id (coach_id = auth.uid()), not a coaches.id — the only coach
/// table keyed this way.
/// </summary>
public class SubscriptionPlan
{
    public Guid Id { get; set; }

    public Guid CoachId { get; set; }

    public string Name { get; set; } = null!;

    public decimal? PriceUsd { get; set; }

    public int? DurationDays { get; set; }

    public int? MaxClients { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? Coach { get; set; }
}
