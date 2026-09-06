namespace CoreGym.Domain.Entities;

/// <summary>
/// Canonical subscription table (decision 2026-09-06): everything including the
/// stripe-webhook function writes here; `coach_subscriptions` is NOT ported.
/// The Authorization Service's IsActiveClientOfCoach queries this table.
/// </summary>
public class Subscription
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }

    /// <summary>References coaches.id (not the user id) — matches the original RLS joins.</summary>
    public Guid CoachId { get; set; }

    /// <summary>'pending' | 'active' | 'cancelled' | 'expired' — enforced by CHECK.</summary>
    public string? Status { get; set; }

    public string? Tier { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? StripeSubId { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? PlanId { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public string? Goals { get; set; }

    public string? Notes { get; set; }

    public Profile? Client { get; set; }

    public Coach? Coach { get; set; }

    public SubscriptionPlan? Plan { get; set; }
}
