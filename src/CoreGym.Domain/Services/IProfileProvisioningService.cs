namespace CoreGym.Domain.Services;

/// <summary>
/// Replaces the handle_new_user() trigger on auth.users: provisions the
/// profiles row for a freshly signed-up user. Idempotent — returns the
/// existing profile when present. Called by the future auth flow.
/// </summary>
public interface IProfileProvisioningService
{
    Task<Entities.Profile> ProvisionAsync(Guid userId, string? email = null, string? name = null, CancellationToken cancellationToken = default);
}
