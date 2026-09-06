using CoreGym.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace CoreGym.Infrastructure.Authorization;

/// <summary>Marker requirement for the reusable OwnDataOrActiveCoach policy.</summary>
public class OwnDataOrActiveCoachRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Single reusable replacement for the recurring RLS pattern
/// "user can see/edit own data OR a coach with an active subscription can see
/// the client's data". Registered once (AddCoreGymAuthorization) and evaluated
/// against any IOwnedResource via the OwnDataOrActiveCoachPolicy — never
/// duplicated per endpoint.
/// </summary>
public class OwnDataOrActiveCoachHandler : AuthorizationHandler<OwnDataOrActiveCoachRequirement, IOwnedResource>
{
    public const string PolicyName = "OwnDataOrActiveCoach";

    private readonly IClientAccessService _clientAccessService;

    public OwnDataOrActiveCoachHandler(IClientAccessService clientAccessService)
    {
        _clientAccessService = clientAccessService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnDataOrActiveCoachRequirement requirement,
        IOwnedResource resource)
    {
        var userId = UserIdClaimReader.GetUserId(context.User);
        if (userId is null)
        {
            return;
        }

        if (resource.UserId == userId.Value)
        {
            context.Succeed(requirement);
            return;
        }

        if (await _clientAccessService.IsActiveClientOfCoach(userId.Value, resource.UserId))
        {
            context.Succeed(requirement);
        }
    }
}
