using CoreGym.Domain.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Authorization;

public class ClientAccessService : IClientAccessService
{
    private readonly CoreGymDbContext _db;

    public ClientAccessService(CoreGymDbContext db)
    {
        _db = db;
    }

    // Mirrors is_my_active_client(client_uid): one query joining the coach's
    // user id through coaches to an ACTIVE subscription for this client.
    public Task<bool> IsActiveClientOfCoach(Guid coachUserId, Guid clientUserId, CancellationToken cancellationToken = default) =>
        _db.Subscriptions
            .AsNoTracking()
            .Where(s => s.Status == "active"
                        && s.ClientId == clientUserId
                        && s.Coach!.UserId == coachUserId)
            .AnyAsync(cancellationToken);
}
