using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using CoreGym.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Tests;

/// <summary>
/// End-to-end Stripe webhook tests: signature verification, checkout.session.completed
/// (activation + payment intent + customer mapping, idempotent on replay) and
/// customer.subscription.updated (status transition).
/// </summary>
[Collection("api-smoke")]
public class StripeWebhookTests
{
    private readonly ApiSmokeFixture _fx;

    public StripeWebhookTests(ApiSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Invalid_signature_is_rejected()
    {
        var body = BuildEvent("checkout.session.completed", new { id = "cs_1", @object = "checkout.session" });
        var response = await PostWebhookAsync(body, "t=1,v1=deadbeef");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_completed_activates_subscription_and_records_payment()
    {
        var clientId = await CreateUserAsync();
        var (coachId, coachUserId) = await CreateCoachAsync();
        var subscription = await CreateSubscriptionAsync(clientId, coachId, "pending");

        var body = BuildEvent("checkout.session.completed", new
        {
            id = "cs_test_1",
            @object = "checkout.session",
            payment_intent = "pi_test_1",
            customer = "cus_test_1",
            subscription = "sub_test_1",
            amount_total = 2999,
            currency = "usd",
            metadata = new { supabase_uid = clientId.ToString(), coach_id = coachId.ToString() },
        });
        var response = await PostWebhookAsync(body);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var ctx = _fx.CreateContext();
        var updated = await ctx.Subscriptions.AsNoTracking().SingleAsync(s => s.Id == subscription);
        Assert.Equal("active", updated.Status);
        Assert.Equal("sub_test_1", updated.StripeSubId);

        var payment = await ctx.PaymentIntents.AsNoTracking().SingleAsync(pi => pi.StripePaymentId == "pi_test_1");
        Assert.Equal(clientId, payment.ClientId);
        Assert.Equal(coachId, payment.CoachId);
        Assert.Equal(2999m, payment.Amount);
        Assert.Equal("succeeded", payment.Status);
        Assert.Equal("cus_test_1", payment.StripeCustomerId);

        var customer = await ctx.StripeCustomers.AsNoTracking().SingleAsync(sc => sc.UserId == clientId);
        Assert.Equal("cus_test_1", customer.StripeCustomerId);

        var conversation = await ctx.Conversations.AsNoTracking().SingleAsync(c => c.SubscriptionId == subscription);
        Assert.Equal(coachUserId, conversation.CoachId);
    }

    [Fact]
    public async Task Replaying_the_same_event_is_idempotent()
    {
        var clientId = await CreateUserAsync();
        var (coachId, coachUserId) = await CreateCoachAsync();
        var subscription = await CreateSubscriptionAsync(clientId, coachId, "pending");
        await CreateCoachProfileAsync(coachUserId);

        var body = BuildEvent("checkout.session.completed", new
        {
            id = "cs_test_2",
            @object = "checkout.session",
            payment_intent = "pi_test_2",
            customer = "cus_test_2",
            amount_total = 1500,
            currency = "usd",
            metadata = new { supabase_uid = clientId.ToString(), coach_id = coachId.ToString() },
        });

        await PostWebhookAsync(body);
        var replay = await PostWebhookAsync(body);
        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);

        await using var ctx = _fx.CreateContext();
        Assert.Equal(1, await ctx.PaymentIntents.CountAsync(pi => pi.StripePaymentId == "pi_test_2"));
        Assert.Equal(1, await ctx.StripeCustomers.CountAsync(sc => sc.StripeCustomerId == "cus_test_2"));
        Assert.Equal(1, (await ctx.CoachProfiles.AsNoTracking().SingleAsync(cp => cp.Id == coachUserId)).CurrentClients);
        Assert.Equal("active", (await ctx.Subscriptions.AsNoTracking().SingleAsync(s => s.Id == subscription)).Status);
    }

    [Fact]
    public async Task Subscription_updated_transitions_status()
    {
        var clientId = await CreateUserAsync();
        var (coachId, coachProfileId) = await CreateCoachAsync();
        var subscription = await CreateSubscriptionAsync(clientId, coachId, "active", stripeSubId: "sub_test_3");
        await CreateCoachProfileAsync(coachProfileId);

        var body = BuildEvent("customer.subscription.updated", new
        {
            id = "sub_test_3",
            @object = "subscription",
            status = "canceled",
        });
        var response = await PostWebhookAsync(body);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var ctx = _fx.CreateContext();
        var updated = await ctx.Subscriptions.AsNoTracking().SingleAsync(s => s.Id == subscription);
        Assert.Equal("cancelled", updated.Status);
        Assert.Equal(0, (await ctx.CoachProfiles.AsNoTracking().SingleAsync(cp => cp.Id == coachProfileId)).CurrentClients);
    }

    private async Task<HttpResponseMessage> PostWebhookAsync(string body, string? signatureOverride = null)
    {
        var client = _fx.CreateAnonymousClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/stripe")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("Stripe-Signature", signatureOverride ?? Sign(body));
        return await client.SendAsync(request);
    }

    private static string BuildEvent(string type, object dataObject)
    {
        var @event = new { id = $"evt_{Guid.NewGuid():N}", type, data = new { @object = dataObject } };
        return JsonSerializer.Serialize(@event);
    }

    private string Sign(string payload)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_fx.StripeWebhookSecret));
        var signature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}.{payload}")))
            .ToLowerInvariant();
        return $"t={timestamp},v1={signature}";
    }

    private async Task<Guid> CreateUserAsync(string email = "stripe-client@example.com")
    {
        await using var ctx = _fx.CreateContext();
        var profile = await new ProfileProvisioningService(ctx).ProvisionAsync(Guid.NewGuid(), email: email, name: "Stripe Client");
        return profile.Id;
    }

    private async Task<(Guid CoachId, Guid CoachUserId)> CreateCoachAsync()
    {
        await using var ctx = _fx.CreateContext();
        var coachUser = await new ProfileProvisioningService(ctx).ProvisionAsync(Guid.NewGuid(), name: "Stripe Coach");
        var coach = new Coach { Id = Guid.NewGuid(), UserId = coachUser.Id };
        ctx.Coaches.Add(coach);
        await ctx.SaveChangesAsync();
        return (coach.Id, coachUser.Id);
    }

    private async Task CreateCoachProfileAsync(Guid coachUserId)
    {
        await using var ctx = _fx.CreateContext();
        if (!await ctx.CoachProfiles.AnyAsync(cp => cp.Id == coachUserId))
        {
            ctx.CoachProfiles.Add(new CoachProfile { Id = coachUserId });
            await ctx.SaveChangesAsync();
        }
    }

    private async Task<Guid> CreateSubscriptionAsync(Guid clientId, Guid coachId, string status, string? stripeSubId = null)
    {
        await using var ctx = _fx.CreateContext();
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CoachId = coachId,
            Status = status,
            StripeSubId = stripeSubId,
        };
        ctx.Subscriptions.Add(subscription);
        await ctx.SaveChangesAsync();
        return subscription.Id;
    }
}
