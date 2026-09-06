namespace CoreGym.Domain.Authorization;

/// <summary>
/// Lightweight IOwnedResource for resource-based authorization when the check
/// is keyed only by a target user id (e.g. a route parameter on coach endpoints).
/// </summary>
public record OwnedResource(Guid UserId) : IOwnedResource;
