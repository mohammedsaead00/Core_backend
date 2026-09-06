using System.Text.Json;
using CoreGym.Domain.Entities;
using CoreGym.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Integrations;

/// <summary>
/// Stripe webhook handling. The checkout metadata contract (INFERRED — the
/// original edge function's exact fields are not in the inventory):
///   supabase_uid  → client user id
///   coach_id      → coaches.id
///   subscription_id → (optional) explicit subscriptions.id
/// </summary>
public class StripeWebhookService : IStripeWebhookService
{
    private readonly CoreGymDbContext _db;
    private readonly ISubscriptionLifecycleService _lifecycle;

    public StripeWebhookService(CoreGymDbContext db, ISubscriptionLifecycleService lifecycle)
    {
        _db = db;
        _lifecycle = lifecycle;
    }

    public async Task HandleEventAsync(string eventJson, CancellationToken cancellationToken = default)
    {
        using var doc = JsonDocument.Parse(eventJson);
        var root = doc.RootElement;
        if (!root.TryGetProperty("type", out var typeElement) || !root.TryGetProperty("data", out var data))
        {
            return;
        }

        var type = typeElement.GetString();
        if (!data.TryGetProperty("object", out var obj) || obj.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        switch (type)
        {
            case "checkout.session.completed":
                await HandleCheckoutCompletedAsync(obj, cancellationToken);
                break;
            case "customer.subscription.updated":
            case "customer.subscription.deleted":
                await HandleSubscriptionStatusAsync(obj, cancellationToken);
                break;
        }
    }

    private async Task HandleCheckoutCompletedAsync(JsonElement session, CancellationToken cancellationToken)
    {
        var metadata = session.TryGetProperty("metadata", out var m) && m.ValueKind == JsonValueKind.Object
            ? m
            : (JsonElement?)null;
        var clientUserId = GetGuid(metadata, "supabase_uid");
        var coachId = GetGuid(metadata, "coach_id");
        var subscriptionId = GetGuid(metadata, "subscription_id");
        var paymentIntentId = GetString(session, "payment_intent");
        var customerId = GetString(session, "customer");
        var amountTotal = GetNumber(session, "amount_total");
        var currency = GetString(session, "currency");
        var stripeSubscriptionId = GetString(session, "subscription");

        // Payment record (idempotent on Stripe's payment intent id — Stripe retries events).
        if (clientUserId is not null && coachId is not null && paymentIntentId is not null
            && !await _db.PaymentIntents.AnyAsync(pi => pi.StripePaymentId == paymentIntentId, cancellationToken))
        {
            _db.PaymentIntents.Add(new PaymentIntent
            {
                Id = Guid.NewGuid(),
                ClientId = clientUserId.Value,
                CoachId = coachId.Value,
                StripePaymentId = paymentIntentId,
                StripeCustomerId = customerId,
                Amount = amountTotal ?? 0m, // Stripe minor units (cents) — see open question 10
                Currency = currency ?? "usd",
                Status = "succeeded",
            });
        }

        // Customer mapping (written by create-checkout-session too; keep idempotent).
        if (clientUserId is not null && customerId is not null
            && !await _db.StripeCustomers.AnyAsync(sc => sc.UserId == clientUserId && sc.StripeCustomerId == customerId, cancellationToken))
        {
            _db.StripeCustomers.Add(new StripeCustomer
            {
                Id = Guid.NewGuid(),
                UserId = clientUserId.Value,
                StripeCustomerId = customerId,
            });
        }

        // Activate the subscription this checkout belongs to.
        Subscription? subscription = null;
        if (subscriptionId is not null)
        {
            subscription = await _db.Subscriptions
                .SingleOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);
        }
        else if (clientUserId is not null && coachId is not null)
        {
            subscription = await _db.Subscriptions
                .Where(s => s.ClientId == clientUserId && s.CoachId == coachId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (subscription is not null)
        {
            if (stripeSubscriptionId is not null)
            {
                subscription.StripeSubId = stripeSubscriptionId;
            }

            await _db.SaveChangesAsync(cancellationToken);
            if (subscription.Status != "active")
            {
                await _lifecycle.ProcessSubscriptionStatusAsync(subscription.Id, "active", cancellationToken);
            }
        }
        else
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task HandleSubscriptionStatusAsync(JsonElement stripeSubscription, CancellationToken cancellationToken)
    {
        var stripeSubId = GetString(stripeSubscription, "id");
        var status = GetString(stripeSubscription, "status");
        if (stripeSubId is null || status is null)
        {
            return;
        }

        // Stripe status → our CHECK-constrained values (mapping inferred).
        var mapped = status switch
        {
            "active" or "trialing" => "active",
            "canceled" or "unpaid" or "incomplete" or "incomplete_expired" => "cancelled",
            "past_due" => "pending",
            _ => null,
        };
        if (mapped is null)
        {
            return;
        }

        var subscription = await _db.Subscriptions
            .SingleOrDefaultAsync(s => s.StripeSubId == stripeSubId, cancellationToken);
        if (subscription is null || subscription.Status == mapped)
        {
            return;
        }

        await _lifecycle.ProcessSubscriptionStatusAsync(subscription.Id, mapped, cancellationToken);
    }

    private static Guid? GetGuid(JsonElement? element, string propertyName)
    {
        var value = GetString(element, propertyName);
        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    private static string? GetString(JsonElement? element, string propertyName)
    {
        if (element is not { ValueKind: JsonValueKind.Object } obj
            || !obj.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static decimal? GetNumber(JsonElement? element, string propertyName)
    {
        if (element is not { ValueKind: JsonValueKind.Object } obj
            || !obj.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Number
            || !property.TryGetInt64(out var value))
        {
            return null;
        }

        return value;
    }
}
