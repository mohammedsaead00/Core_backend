namespace CoreGym.Domain.Services;

/// <summary>
/// Handles Stripe webhook events (signature is verified by the caller before
/// this runs): checkout.session.completed activates the subscription and
/// records the payment; customer.subscription.updated/deleted transitions it.
/// </summary>
public interface IStripeWebhookService
{
    /// <summary>Processes a verified Stripe event body. Unknown event types are ignored.</summary>
    Task HandleEventAsync(string eventJson, CancellationToken cancellationToken = default);
}
