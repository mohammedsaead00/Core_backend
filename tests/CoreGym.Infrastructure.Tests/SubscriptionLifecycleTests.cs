using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using CoreGym.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Verifies the handle_subscription_accepted replacement: activation creates
/// the coach↔client conversation and bumps coach_profiles.current_clients;
/// cancel/expire decrements (floored at zero); repeat transitions are no-ops.
/// </summary>
[Collection("sql-smoke")]
public class SubscriptionLifecycleTests
{
    private readonly SqlServerSmokeFixture _fx;

    public SubscriptionLifecycleTests(SqlServerSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Activation_creates_conversation_and_increments_current_clients()
    {
        var (subscriptionId, coachProfileId) = await CreateSubscriptionAsync(_fx.Context, "pending");
        var coachUserId = await _fx.Context.CoachProfiles
            .Where(cp => cp.Id == coachProfileId)
            .Select(cp => cp.Id)
            .SingleAsync();

        await using var ctx = _fx.CreateContext();
        await new SubscriptionLifecycleService(ctx).ProcessSubscriptionStatusAsync(subscriptionId, "active");

        await using var ctx2 = _fx.CreateContext();
        var subscription = await ctx2.Subscriptions.AsNoTracking().SingleAsync(s => s.Id == subscriptionId);
        Assert.Equal("active", subscription.Status);

        var conversation = await ctx2.Conversations.AsNoTracking().SingleAsync();
        Assert.Equal(conversation.ClientId, await ctx2.Subscriptions.Where(s => s.Id == subscriptionId).Select(s => s.ClientId).SingleAsync());
        Assert.Equal(coachUserId, conversation.CoachId); // coach USER id, not coaches.id
        Assert.Equal(subscriptionId, conversation.SubscriptionId);

        Assert.Equal(1, (await ctx2.CoachProfiles.AsNoTracking().SingleAsync(cp => cp.Id == coachProfileId)).CurrentClients);
    }

    [Fact]
    public async Task Repeated_activation_is_a_noop()
    {
        var (subscriptionId, coachProfileId) = await CreateSubscriptionAsync(_fx.Context, "pending");

        await using var ctx = _fx.CreateContext();
        var service = new SubscriptionLifecycleService(ctx);
        await service.ProcessSubscriptionStatusAsync(subscriptionId, "active");
        await service.ProcessSubscriptionStatusAsync(subscriptionId, "active");

        await using var ctx2 = _fx.CreateContext();
        Assert.Equal(1, (await ctx2.CoachProfiles.AsNoTracking().SingleAsync(cp => cp.Id == coachProfileId)).CurrentClients);
        Assert.Equal(1, await ctx2.Conversations.AsNoTracking().CountAsync(c => c.SubscriptionId == subscriptionId));
    }

    [Fact]
    public async Task Cancel_decrements_and_repeated_cancel_stays_floored_at_zero()
    {
        var (subscriptionId, coachProfileId) = await CreateSubscriptionAsync(_fx.Context, "pending");

        await using var ctx = _fx.CreateContext();
        var service = new SubscriptionLifecycleService(ctx);
        await service.ProcessSubscriptionStatusAsync(subscriptionId, "active");
        await service.ProcessSubscriptionStatusAsync(subscriptionId, "cancelled");
        await service.ProcessSubscriptionStatusAsync(subscriptionId, "cancelled");
        await service.ProcessSubscriptionStatusAsync(subscriptionId, "expired");

        await using var ctx2 = _fx.CreateContext();
        Assert.Equal(0, (await ctx2.CoachProfiles.AsNoTracking().SingleAsync(cp => cp.Id == coachProfileId)).CurrentClients);
        Assert.Equal("expired", (await ctx2.Subscriptions.AsNoTracking().SingleAsync(s => s.Id == subscriptionId)).Status);
    }

    [Fact]
    public async Task Invalid_status_and_unknown_subscription_are_rejected()
    {
        var (subscriptionId, _) = await CreateSubscriptionAsync(_fx.Context, "pending");

        await using var ctx = _fx.CreateContext();
        var service = new SubscriptionLifecycleService(ctx);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ProcessSubscriptionStatusAsync(subscriptionId, "paused"));
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ProcessSubscriptionStatusAsync(Guid.NewGuid(), "active"));
    }

    private static async Task<(Guid SubscriptionId, Guid CoachProfileId)> CreateSubscriptionAsync(CoreGymDbContext ctx, string status)
    {
        var client = new Profile { Id = Guid.NewGuid(), Name = "sub-client" };
        var coachUser = new Profile { Id = Guid.NewGuid(), Name = "sub-coach" };
        var coach = new Coach { Id = Guid.NewGuid(), UserId = coachUser.Id };
        var coachProfile = new CoachProfile { Id = coachUser.Id };
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            CoachId = coach.Id,
            Status = status,
        };
        ctx.AddRange(client, coachUser, coach, coachProfile, subscription);
        await ctx.SaveChangesAsync();
        return (subscription.Id, coachProfile.Id);
    }
}
