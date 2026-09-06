using CoreGym.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreGym.Api.Controllers;

/// <summary>
/// Base class for authenticated controllers. The user id always comes from the
/// JWT (ICurrentUserService) — never from the client — so a user can only ever
/// address their own data except through the policy-guarded coach endpoints.
/// </summary>
public abstract class CoreGymControllerBase : ControllerBase
{
    private readonly ICurrentUserService _currentUser;

    protected CoreGymControllerBase(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>Throws (mapped to 403) when a valid token carries no usable user id.</summary>
    protected Guid UserId =>
        _currentUser.UserId
        ?? throw new UnauthorizedAccessException("The token does not carry a user id (sub claim).");
}
