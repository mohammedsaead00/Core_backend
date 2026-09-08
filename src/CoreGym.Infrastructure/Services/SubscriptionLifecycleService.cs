using CoreGym.Domain.Entities;
using CoreGym.Domain.Services;
using CoreGym.Infrastructure.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CoreGym.Infrastructure.Services;

public class SubscriptionLifecycleService : ISubscriptionLifecycleService
{
    private readonly CoreGymDbContext _db;
    private readonly IMemoryCache _cache;

    public SubscriptionLifecycleService(CoreGymDbContext db, IMemoryCache? cache = null)
    {
        _db = db;
        _cache = cache ?? new MemoryCache(new MemoryCacheOptions());
    }

    public async Task ProcessSubscriptionStatusAsync(Guid subscriptionId, string newStatus, CancellationToken cancellationToken = default)
    {
        var normalized = newStatus?.Trim().ToLowerInvariant()
            ?? throw new ArgumentException("Subscription status is required.", nameof(newStatus));
        if (normalized is not ("pending" or "active" or "cancelled" or "expired"))
        {
            throw new ArgumentException($"Status must be pending, active, cancelled or expired.", nameof(newStatus));
        }

        var subscription = await _db.Subscriptions
            .Include(s => s.Coach)
            .SingleOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Subscription '{subscriptionId}' was not found.");

        var previous = subscription.Status?.ToLowerInvariant();
        if (previous == normalized)
        {
            return;
        }

        subscription.Status = normalized;

        // Evict the authorization cache so coach access changes take effect immediately.
        _cache.Remove(ClientAccessService.CacheKey(subscription.Coach!.UserId, subscription.ClientId));

        if (normalized == "active")
        {
            await EnsureConversationAndCountClientAsync(subscription, increment: true, cancellationToken);
        }
        else if (previous == "active" && normalized is "cancelled" or "expired")
        {
            await EnsureConversationAndCountClientAsync(subscription, increment: false, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureConversationAndCountClientAsync(Subscription subscription, bool increment, CancellationToken cancellationToken)
    {
        // conversations.coach_id holds the coach's AUTH USER id.
        var coachUserId = subscription.Coach!.UserId;

        var profile = await _db.CoachProfiles.SingleOrDefaultAsync(cp => cp.Id == coachUserId, cancellationToken);
        if (increment)
        {
            var conversation = await _db.Conversations.SingleOrDefaultAsync(
                c => c.ClientId == subscription.ClientId && c.CoachId == coachUserId, cancellationToken);
            if (conversation is null)
            {
                _db.Conversations.Add(new Conversation
                {
                    Id = Guid.NewGuid(),
                    ClientId = subscription.ClientId,
                    CoachId = coachUserId,
                    SubscriptionId = subscription.Id,
                });
            }
            else
            {
                conversation.SubscriptionId ??= subscription.Id;
                conversation.IsActive = true;
            }

            if (profile is not null)
            {
                profile.CurrentClients++;
            }
        }
        else if (profile is not null && profile.CurrentClients > 0)
        {
            profile.CurrentClients--;
        }
    }
}
