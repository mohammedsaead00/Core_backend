namespace CoreGym.Domain.Authorization;

/// <summary>
/// Implemented by every entity that carries a personal user_id column and
/// therefore falls under the OwnDataOrActiveCoach authorization policy.
/// </summary>
public interface IOwnedResource
{
    Guid UserId { get; }
}
