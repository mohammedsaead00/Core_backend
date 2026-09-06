using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using CoreGym.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Verifies the sync_nutrition_to_summary / sync_workout_to_summary
/// replacements: totals roll up into daily_summary and re-sync after changes.
/// </summary>
[Collection("sql-smoke")]
public class DailySummaryServiceTests
{
    private readonly SqlServerSmokeFixture _fx;

    public DailySummaryServiceTests(SqlServerSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Nutrition_sync_creates_summary_with_day_totals()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var day = DateTime.UtcNow.Date;
        _fx.Context.NutritionLogs.AddRange(
            new NutritionLog { Id = Guid.NewGuid(), UserId = userId, FoodName = "Oats", Calories = 300m, ProteinG = 30m, CarbsG = 10m, FatG = 5m, LoggedDate = day },
            new NutritionLog { Id = Guid.NewGuid(), UserId = userId, FoodName = "Chicken", Calories = 500m, ProteinG = 20m, CarbsG = 5m, FatG = 10m, LoggedDate = day });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        await new DailySummaryService(ctx).SyncNutritionAsync(userId, day);

        await using var ctx2 = _fx.CreateContext();
        var summary = await ctx2.DailySummaries.AsNoTracking().SingleAsync(s => s.UserId == userId);
        Assert.Equal(800m, summary.CaloriesConsumed);
        Assert.Equal(50m, summary.ProteinG);
        Assert.Equal(15m, summary.CarbsG);
        Assert.Equal(15m, summary.FatG);
    }

    [Fact]
    public async Task Nutrition_resync_reflects_deletions()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var day = DateTime.UtcNow.Date;
        var first = new NutritionLog { Id = Guid.NewGuid(), UserId = userId, FoodName = "Oats", Calories = 300m, ProteinG = 30m, LoggedDate = day };
        var second = new NutritionLog { Id = Guid.NewGuid(), UserId = userId, FoodName = "Rice", Calories = 200m, ProteinG = 10m, LoggedDate = day };
        _fx.Context.NutritionLogs.AddRange(first, second);
        await _fx.Context.SaveChangesAsync();

        await using (var ctx = _fx.CreateContext())
        {
            await new DailySummaryService(ctx).SyncNutritionAsync(userId, day);
        }

        await using (var ctx = _fx.CreateContext())
        {
            ctx.NutritionLogs.Remove(await ctx.NutritionLogs.SingleAsync(n => n.Id == second.Id));
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = _fx.CreateContext())
        {
            await new DailySummaryService(ctx).SyncNutritionAsync(userId, day);
        }

        await using var ctx2 = _fx.CreateContext();
        var summary = await ctx2.DailySummaries.AsNoTracking().SingleAsync(s => s.UserId == userId);
        Assert.Equal(300m, summary.CaloriesConsumed);
        Assert.Equal(30m, summary.ProteinG);
    }

    [Fact]
    public async Task Workout_sync_sets_done_flag_and_total_duration()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var day = DateTime.UtcNow.Date;
        _fx.Context.WorkoutSessions.AddRange(
            new WorkoutSession { Id = Guid.NewGuid(), UserId = userId, MuscleGroup = "chest", DurationMin = 45, SessionDate = day },
            new WorkoutSession { Id = Guid.NewGuid(), UserId = userId, MuscleGroup = "back", DurationMin = 30, SessionDate = day });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        await new DailySummaryService(ctx).SyncWorkoutAsync(userId, day);

        await using var ctx2 = _fx.CreateContext();
        var summary = await ctx2.DailySummaries.AsNoTracking().SingleAsync(s => s.UserId == userId);
        Assert.True(summary.WorkoutDone);
        Assert.Equal(75, summary.WorkoutDuration);
    }

    [Fact]
    public async Task Workout_resync_after_session_delete_clears_the_flag()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var day = DateTime.UtcNow.Date;
        var session = new WorkoutSession { Id = Guid.NewGuid(), UserId = userId, MuscleGroup = "legs", DurationMin = 60, SessionDate = day };
        _fx.Context.WorkoutSessions.Add(session);
        await _fx.Context.SaveChangesAsync();

        await using (var ctx = _fx.CreateContext())
        {
            await new DailySummaryService(ctx).SyncWorkoutAsync(userId, day);
        }

        await using (var ctx = _fx.CreateContext())
        {
            ctx.WorkoutSessions.Remove(await ctx.WorkoutSessions.SingleAsync(s => s.Id == session.Id));
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = _fx.CreateContext())
        {
            await new DailySummaryService(ctx).SyncWorkoutAsync(userId, day);
        }

        await using var ctx2 = _fx.CreateContext();
        var summary = await ctx2.DailySummaries.AsNoTracking().SingleAsync(s => s.UserId == userId);
        Assert.False(summary.WorkoutDone);
        Assert.Equal(0, summary.WorkoutDuration);
    }

    [Fact]
    public async Task Sync_without_any_data_does_not_create_a_summary_row()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var day = DateTime.UtcNow.Date;

        await using var ctx = _fx.CreateContext();
        await new DailySummaryService(ctx).SyncNutritionAsync(userId, day);
        await new DailySummaryService(ctx).SyncWorkoutAsync(userId, day);

        Assert.False(await ctx.DailySummaries.AnyAsync(s => s.UserId == userId));
    }

    private static async Task<Guid> CreateUserAsync(CoreGymDbContext ctx)
    {
        var profile = new Profile { Id = Guid.NewGuid(), Name = "summary-user" };
        ctx.Profiles.Add(profile);
        await ctx.SaveChangesAsync();
        return profile.Id;
    }
}
