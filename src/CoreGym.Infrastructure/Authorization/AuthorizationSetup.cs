using CoreGym.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CoreGym.Infrastructure.Authorization;

public static class AuthorizationSetup
{
    public const string OwnDataOrActiveCoachPolicy = "OwnDataOrActiveCoach";

    /// <summary>
    /// Registers the Phase 1 authorization stack: ICurrentUserService,
    /// IClientAccessService (queries the canonical subscriptions table) and the
    /// reusable OwnDataOrActiveCoach resource-based policy.
    /// </summary>
    public static IServiceCollection AddCoreGymAuthorization(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IClientAccessService, ClientAccessService>();
        services.AddScoped<IAuthorizationHandler, OwnDataOrActiveCoachHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(OwnDataOrActiveCoachPolicy, policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new OwnDataOrActiveCoachRequirement()));
        });

        return services;
    }
}
