namespace CoreGym.Domain.Authorization;

/// <summary>
/// Equivalent of Supabase's auth.uid(): the id of the user making the current
/// request, resolved from their JWT claims. The exact claim mapping depends on
/// the auth provider that replaces Supabase Auth (still an open decision) —
/// the implementation centralizes that in one place.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
}
