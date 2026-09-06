namespace CoreGym.Domain.Entities;

/// <summary>Stripe payment record written by the stripe-webhook function.</summary>
public class PaymentIntent
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }

    /// <summary>References coaches.id (per the original RLS coach-read policy).</summary>
    public Guid CoachId { get; set; }

    public string StripePaymentId { get; set; } = null!;

    public string? StripeCustomerId { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public string? Status { get; set; }

    public string? Tier { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? Client { get; set; }

    public Coach? Coach { get; set; }
}
