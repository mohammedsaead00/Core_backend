using System.Security.Claims;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using CoreGym.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Integration tests for the Phase 1 authorization stack — the reusable
/// replacement for the "own data OR active coach" RLS pattern, evaluated
/// through the real IAuthorizationService pipeline against the scratch database.
/// </summary>
[Collection("sql-smoke")]
public class AuthorizationServiceTests
{
    private readonly SqlServerSmokeFixture _fx;

    public AuthorizationServiceTests(SqlServerSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Owner_can_access_their_own_resource()
    {
        var clientId = await CreateUserAsync(_fx.Context);
        var resource = new BodyMeasurement { Id = Guid.NewGuid(), UserId = clientId };

        var result = await AuthorizeAsync(PrincipalFor(clientId), resource);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Coach_with_active_subscription_can_access_client_resource()
    {
        var clientId = await CreateUserAsync(_fx.Context);
        var coachUserId = await CreateCoachWithSubscriptionAsync(_fx.Context, clientId, "active");
        var resource = new DailySummary { Id = Guid.NewGuid(), UserId = clientId };

        var result = await AuthorizeAsync(PrincipalFor(coachUserId), resource);

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("cancelled")]
    [InlineData("expired")]
    public async Task Coach_without_active_subscription_is_denied(string status)
    {
        var clientId = await CreateUserAsync(_fx.Context);
        var coachUserId = await CreateCoachWithSubscriptionAsync(_fx.Context, clientId, status);
        var resource = new NutritionLog { Id = Guid.NewGuid(), UserId = clientId, FoodName = "Rice", Calories = 200m };

        var result = await AuthorizeAsync(PrincipalFor(coachUserId), resource);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Unrelated_user_is_denied()
    {
        var clientId = await CreateUserAsync(_fx.Context);
        var strangerId = await CreateUserAsync(_fx.Context);
        var resource = new WorkoutSession { Id = Guid.NewGuid(), UserId = clientId, MuscleGroup = "legs" };

        var result = await AuthorizeAsync(PrincipalFor(strangerId), resource);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Principal_without_user_id_claim_is_denied()
    {
        var clientId = await CreateUserAsync(_fx.Context);
        var resource = new WorkoutSet { Id = Guid.NewGuid(), UserId = clientId, ExerciseName = "Row", SetNumber = 1 };

        var anonymous = new ClaimsPrincipal();
        var authenticatedWithoutClaim = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("name", "no-sub") }, "test"));

        Assert.False((await AuthorizeAsync(anonymous, resource)).Succeeded);
        Assert.False((await AuthorizeAsync(authenticatedWithoutClaim, resource)).Succeeded);
    }

    [Fact]
    public async Task IsActiveClientOfCoach_matches_only_the_exact_active_pair()
    {
        var clientId = await CreateUserAsync(_fx.Context);
        var otherClientId = await CreateUserAsync(_fx.Context);
        var coachUserId = await CreateCoachWithSubscriptionAsync(_fx.Context, clientId, "active");
        var secondCoachUserId = await CreateCoachWithSubscriptionAsync(_fx.Context, otherClientId, "active");

        await using var ctx = _fx.CreateContext();
        var access = new ClientAccessService(ctx);

        Assert.True(await access.IsActiveClientOfCoach(coachUserId, clientId));
        Assert.False(await access.IsActiveClientOfCoach(coachUserId, otherClientId));
        Assert.False(await access.IsActiveClientOfCoach(secondCoachUserId, clientId));

        // The coach's own user id is not a client of themselves without a subscription.
        Assert.False(await access.IsActiveClientOfCoach(coachUserId, coachUserId));
    }

    private async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal principal, CoreGym.Domain.Authorization.IOwnedResource resource)
    {
        var httpContext = new DefaultHttpContext { User = principal };

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = httpContext });
        services.AddDbContext<CoreGymDbContext>(options => options.UseSqlServer(_fx.ConnectionString));
        services.AddCoreGymAuthorization();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

        return await authorizationService.AuthorizeAsync(principal, resource, AuthorizationSetup.OwnDataOrActiveCoachPolicy);
    }

    private static ClaimsPrincipal PrincipalFor(Guid userId) =>
        new(new ClaimsIdentity(new[] { new Claim("sub", userId.ToString()) }, "test"));

    private static async Task<Guid> CreateUserAsync(CoreGymDbContext ctx, string name = "auth-user")
    {
        var profile = new Profile { Id = Guid.NewGuid(), Name = name };
        ctx.Profiles.Add(profile);
        await ctx.SaveChangesAsync();
        return profile.Id;
    }

    /// <summary>client profile + coach profile + coaches row + subscription with the given status.</summary>
    private static async Task<Guid> CreateCoachWithSubscriptionAsync(CoreGymDbContext ctx, Guid clientId, string status)
    {
        var coachUserId = await CreateUserAsync(ctx, "auth-coach");
        var coach = new Coach { Id = Guid.NewGuid(), UserId = coachUserId };
        ctx.Coaches.Add(coach);
        ctx.Subscriptions.Add(new Subscription
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CoachId = coach.Id,
            Status = status,
        });
        await ctx.SaveChangesAsync();
        return coachUserId;
    }
}
