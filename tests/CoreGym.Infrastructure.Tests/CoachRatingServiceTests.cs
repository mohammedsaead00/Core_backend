using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using CoreGym.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Verifies the update_coach_rating / refresh_coach_rating replacement:
/// ratings and review counts recompute as the reviews average on both
/// coaches and coach_profiles.
/// </summary>
[Collection("sql-smoke")]
public class CoachRatingServiceTests
{
    private readonly SqlServerSmokeFixture _fx;

    public CoachRatingServiceTests(SqlServerSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Ratings_recompute_as_the_reviews_average()
    {
        var coachId = await CreateCoachAsync(_fx.Context);
        _fx.Context.Reviews.AddRange(
            new Review { Id = Guid.NewGuid(), ClientId = await CreateClientAsync(_fx.Context), CoachId = coachId, Rating = 5 },
            new Review { Id = Guid.NewGuid(), ClientId = await CreateClientAsync(_fx.Context), CoachId = coachId, Rating = 4 });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        await new CoachRatingService(ctx).RecalculateCoachRatingAsync(coachId);

        await using var ctx2 = _fx.CreateContext();
        var coach = await ctx2.Coaches.AsNoTracking().SingleAsync(c => c.Id == coachId);
        Assert.Equal(4.5m, coach.Rating);
        var profile = await ctx2.CoachProfiles.AsNoTracking().SingleAsync(cp => cp.Id == coach.UserId);
        Assert.Equal(4.5m, profile.Rating);
        Assert.Equal(2, profile.ReviewsCount);
    }

    [Fact]
    public async Task Deletion_of_all_reviews_resets_rating_to_zero()
    {
        var coachId = await CreateCoachAsync(_fx.Context);
        var review = new Review { Id = Guid.NewGuid(), ClientId = await CreateClientAsync(_fx.Context), CoachId = coachId, Rating = 3 };
        _fx.Context.Reviews.Add(review);
        await _fx.Context.SaveChangesAsync();

        await using (var ctx = _fx.CreateContext())
        {
            await new CoachRatingService(ctx).RecalculateCoachRatingAsync(coachId);
        }

        await using (var ctx2 = _fx.CreateContext())
        {
            ctx2.Reviews.Remove(await ctx2.Reviews.SingleAsync(r => r.Id == review.Id));
            await ctx2.SaveChangesAsync();
        }

        await using (var ctx3 = _fx.CreateContext())
        {
            await new CoachRatingService(ctx3).RecalculateCoachRatingAsync(coachId);
        }

        await using var ctx4 = _fx.CreateContext();
        var coach = await ctx4.Coaches.AsNoTracking().SingleAsync(c => c.Id == coachId);
        Assert.Equal(0m, coach.Rating);
        var profile = await ctx4.CoachProfiles.AsNoTracking().SingleAsync(cp => cp.Id == coach.UserId);
        Assert.Equal(0m, profile.Rating);
        Assert.Equal(0, profile.ReviewsCount);
    }

    [Fact]
    public async Task Unknown_coach_is_rejected()
    {
        await using var ctx = _fx.CreateContext();
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => new CoachRatingService(ctx).RecalculateCoachRatingAsync(Guid.NewGuid()));
    }

    private static async Task<Guid> CreateClientAsync(CoreGymDbContext ctx)
    {
        var profile = new Profile { Id = Guid.NewGuid(), Name = "reviewer" };
        ctx.Profiles.Add(profile);
        await ctx.SaveChangesAsync();
        return profile.Id;
    }

    private static async Task<Guid> CreateCoachAsync(CoreGymDbContext ctx)
    {
        var coachUser = new Profile { Id = Guid.NewGuid(), Name = "rated-coach" };
        var coach = new Coach { Id = Guid.NewGuid(), UserId = coachUser.Id };
        ctx.AddRange(coachUser, coach, new CoachProfile { Id = coachUser.Id });
        await ctx.SaveChangesAsync();
        return coach.Id;
    }
}
