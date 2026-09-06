namespace CoreGym.Domain.Authorization;

/// <summary>
/// Exact replacement for the is_my_active_client(client_uid) SQL function used
/// across the original RLS policies: does the given coach (identified by auth
/// user id) have an ACTIVE subscription linking them to the given client?
/// Single query against the canonical `subscriptions` table.
/// </summary>
public interface IClientAccessService
{
    Task<bool> IsActiveClientOfCoach(Guid coachUserId, Guid clientUserId, CancellationToken cancellationToken = default);
}
