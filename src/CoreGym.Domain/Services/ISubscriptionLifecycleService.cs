namespace CoreGym.Domain.Services;

/// <summary>
/// Replaces the handle_subscription_accepted() trigger: on a transition to
/// 'active' it creates/updates the coach↔client conversation and increments
/// coach_profiles.current_clients; on a transition from 'active' to
/// 'cancelled'/'expired' it decrements the counter (floored at zero).
/// Also updates subscriptions.status itself — the future Stripe webhook
/// endpoint calls this with the status from the Stripe event.
/// </summary>
public interface ISubscriptionLifecycleService
{
    /// <summary>Applies the status transition and its side effects. No-op when the status is unchanged.</summary>
    Task ProcessSubscriptionStatusAsync(Guid subscriptionId, string newStatus, CancellationToken cancellationToken = default);
}
