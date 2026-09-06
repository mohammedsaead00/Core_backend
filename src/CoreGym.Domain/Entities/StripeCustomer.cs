namespace CoreGym.Domain.Entities;

/// <summary>Maps an app user to their Stripe customer id (written by create-checkout-session).</summary>
public class StripeCustomer
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string StripeCustomerId { get; set; } = null!;

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? User { get; set; }
}
