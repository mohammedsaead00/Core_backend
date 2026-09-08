using CoreGym.Domain.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CoreGym.Infrastructure.Authorization;

public class ClientAccessService : IClientAccessService
{
    public static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    private readonly CoreGymDbContext _db;
    private readonly IMemoryCache _cache;

    public ClientAccessService(CoreGymDbContext db, IMemoryCache? cache = null)
    {
        _db = db;
        _cache = cache ?? new MemoryCache(new MemoryCacheOptions());
    }

    /// <summary>Cache key shared with the subscription lifecycle service, which evicts on status changes.</summary>
    public static string CacheKey(Guid coachUserId, Guid clientUserId) =>
        $"client-access:{coachUserId}:{clientUserId}";

    // Mirrors is_my_active_client(client_uid): one query joining the coach's
    // user id through coaches to an ACTIVE subscription for this client.
    // Cached briefly (60 s) because the authorization handler runs on every
    // coach request; SubscriptionLifecycleService evicts on every transition,
    // so revocation is immediate for lifecycle-driven changes.
    public Task<bool> IsActiveClientOfCoach(Guid coachUserId, Guid clientUserId, CancellationToken cancellationToken = default) =>
        _cache.GetOrCreateAsync(CacheKey(coachUserId, clientUserId), entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return QueryAsync(coachUserId, clientUserId, cancellationToken);
        });

    private Task<bool> QueryAsync(Guid coachUserId, Guid clientUserId, CancellationToken cancellationToken) =>
        _db.Subscriptions
            .AsNoTracking()
            .Where(s => s.Status == "active"
                        && s.ClientId == clientUserId
                        && s.Coach!.UserId == coachUserId)
            .AnyAsync(cancellationToken);
}
