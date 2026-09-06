using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using CoreGym.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Verifies the record_daily_activity / get_streak_status replacement against
/// the real database: consecutive-day increments, the once-a-month freeze
/// forgiving exactly one missed day, resets, idempotency and status flags.
/// </summary>
[Collection("sql-smoke")]
public class StreakServiceTests
{
    private readonly SqlServerSmokeFixture _fx;

    public StreakServiceTests(SqlServerSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task First_activity_creates_streak_of_one_and_log_row()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var day = UtcToday().AddDays(-5);
        var service = new StreakService(_fx.Context);

        await service.RecordDailyActivityAsync(userId, "workout", day);

        await using var ctx = _fx.CreateContext();
        var streak = await ctx.UserStreaks.AsNoTracking().SingleAsync(s => s.UserId == userId);
        Assert.Equal(1, streak.CurrentStreak);
        Assert.Equal(1, streak.LongestStreak);
        Assert.Equal(day, streak.LastActiveDate!.Value);
        Assert.Equal(1, streak.FreezeAvailable);

        var log = await ctx.StreakActivityLogs.AsNoTracking().SingleAsync(l => l.UserId == userId);
        Assert.Equal("workout", log.Source);
        Assert.Equal(day, log.ActivityDate);
    }

    [Fact]
    public async Task Consecutive_days_increment_the_streak()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var day1 = UtcToday().AddDays(-3);
        var service = new StreakService(_fx.Context);

        await service.RecordDailyActivityAsync(userId, "workout", day1);
        await using var ctx2 = _fx.CreateContext();
        await new StreakService(ctx2).RecordDailyActivityAsync(userId, "nutrition", day1.AddDays(1));

        await using var ctx = _fx.CreateContext();
        var streak = await ctx.UserStreaks.AsNoTracking().SingleAsync(s => s.UserId == userId);
        Assert.Equal(2, streak.CurrentStreak);
        Assert.Equal(2, streak.LongestStreak);
    }

    [Fact]
    public async Task Single_missed_day_is_forgiven_by_consuming_the_freeze()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var day1 = UtcToday().AddDays(-4);
        var service = new StreakService(_fx.Context);

        await service.RecordDailyActivityAsync(userId, "workout", day1);
        await using var ctx2 = _fx.CreateContext();
        await new StreakService(ctx2).RecordDailyActivityAsync(userId, "workout", day1.AddDays(2)); // missed one day

        await using var ctx = _fx.CreateContext();
        var streak = await ctx.UserStreaks.AsNoTracking().SingleAsync(s => s.UserId == userId);
        Assert.Equal(2, streak.CurrentStreak);
        Assert.Equal(2, streak.LongestStreak);
        Assert.Equal(0, streak.FreezeAvailable);
    }

    [Fact]
    public async Task Missed_day_without_freeze_resets_to_one()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var day1 = UtcToday().AddDays(-4);
        var service = new StreakService(_fx.Context);

        await service.RecordDailyActivityAsync(userId, "workout", day1);

        await using var ctx2 = _fx.CreateContext();
        (await ctx2.UserStreaks.SingleAsync(s => s.UserId == userId)).FreezeAvailable = 0;
        await ctx2.SaveChangesAsync();

        await using var ctx3 = _fx.CreateContext();
        await new StreakService(ctx3).RecordDailyActivityAsync(userId, "workout", day1.AddDays(2));

        await using var ctx = _fx.CreateContext();
        var streak = await ctx.UserStreaks.AsNoTracking().SingleAsync(s => s.UserId == userId);
        Assert.Equal(1, streak.CurrentStreak);
        Assert.Equal(0, streak.FreezeAvailable);
    }

    [Fact]
    public async Task Gap_of_two_missed_days_resets_even_with_freeze_available()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var day1 = UtcToday().AddDays(-6);
        var service = new StreakService(_fx.Context);

        await service.RecordDailyActivityAsync(userId, "workout", day1);
        await using var ctx2 = _fx.CreateContext();
        await new StreakService(ctx2).RecordDailyActivityAsync(userId, "workout", day1.AddDays(3)); // missed two days

        await using var ctx = _fx.CreateContext();
        var streak = await ctx.UserStreaks.AsNoTracking().SingleAsync(s => s.UserId == userId);
        Assert.Equal(1, streak.CurrentStreak);
        Assert.Equal(1, streak.FreezeAvailable); // freeze untouched
    }

    [Fact]
    public async Task Second_source_on_the_same_day_is_idempotent_for_the_streak()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var day = UtcToday().AddDays(-2);
        var service = new StreakService(_fx.Context);

        await service.RecordDailyActivityAsync(userId, "workout", day);
        await using var ctx2 = _fx.CreateContext();
        await new StreakService(ctx2).RecordDailyActivityAsync(userId, "nutrition", day);

        await using var ctx = _fx.CreateContext();
        var streak = await ctx.UserStreaks.AsNoTracking().SingleAsync(s => s.UserId == userId);
        Assert.Equal(1, streak.CurrentStreak);
        Assert.Equal(2, await ctx.StreakActivityLogs.CountAsync(l => l.UserId == userId));
    }

    [Fact]
    public async Task Longest_streak_survives_a_later_reset()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var day1 = UtcToday().AddDays(-12);
        var service = new StreakService(_fx.Context);

        for (var i = 0; i < 3; i++)
        {
            await using var ctx = _fx.CreateContext();
            await new StreakService(ctx).RecordDailyActivityAsync(userId, "workout", day1.AddDays(i));
        }

        await using var ctx2 = _fx.CreateContext();
        await new StreakService(ctx2).RecordDailyActivityAsync(userId, "workout", day1.AddDays(10));

        await using var ctx3 = _fx.CreateContext();
        var streak = await ctx3.UserStreaks.AsNoTracking().SingleAsync(s => s.UserId == userId);
        Assert.Equal(1, streak.CurrentStreak);
        Assert.Equal(3, streak.LongestStreak);
    }

    [Fact]
    public async Task Status_reports_logged_flags_and_at_risk()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var yesterday = UtcToday().AddDays(-1);
        var service = new StreakService(_fx.Context);

        Assert.Null(await service.GetStreakStatusAsync(userId));

        await service.RecordDailyActivityAsync(userId, "workout", yesterday);
        await using var ctx2 = _fx.CreateContext();
        var status = await new StreakService(ctx2).GetStreakStatusAsync(userId);

        Assert.NotNull(status);
        Assert.Equal(1, status.CurrentStreak);
        Assert.True(status.AtRisk);        // alive but nothing logged today
        Assert.False(status.LoggedWorkoutToday);
        Assert.False(status.LoggedNutritionToday);
        Assert.Equal(1, status.FreezeAvailable);

        await using var ctx3 = _fx.CreateContext();
        await new StreakService(ctx3).RecordDailyActivityAsync(userId, "nutrition");
        await using var ctx4 = _fx.CreateContext();
        var afterToday = await new StreakService(ctx4).GetStreakStatusAsync(userId);

        Assert.True(afterToday!.LoggedNutritionToday);
        Assert.False(afterToday.AtRisk);
        Assert.Equal(2, afterToday.CurrentStreak);
    }

    [Fact]
    public async Task Invalid_source_is_rejected()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var service = new StreakService(_fx.Context);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.RecordDailyActivityAsync(userId, "meditation"));
    }

    [Fact]
    public async Task Monthly_freeze_reset_only_touches_values_below_one()
    {
        var userA = await CreateUserAsync(_fx.Context, "freeze-a");
        var userB = await CreateUserAsync(_fx.Context, "freeze-b");

        await using (var setup = _fx.CreateContext())
        {
            setup.UserStreaks.AddRange(
                new UserStreak { UserId = userA, CurrentStreak = 1, FreezeAvailable = 0 },
                new UserStreak { UserId = userB, CurrentStreak = 1, FreezeAvailable = 2 });
            await setup.SaveChangesAsync();
        }

        await using var ctx3 = _fx.CreateContext();
        var reset = await new StreakService(ctx3).ResetMonthlyFreezesAsync();
        Assert.Equal(1, reset);

        await using var ctx = _fx.CreateContext();
        Assert.Equal(1, (await ctx.UserStreaks.AsNoTracking().SingleAsync(s => s.UserId == userA)).FreezeAvailable);
        Assert.Equal(2, (await ctx.UserStreaks.AsNoTracking().SingleAsync(s => s.UserId == userB)).FreezeAvailable);
    }

    private static DateTime UtcToday() => DateTime.UtcNow.Date;

    private static async Task<Guid> CreateUserAsync(CoreGymDbContext ctx, string name = "streak-user")
    {
        var profile = new Profile { Id = Guid.NewGuid(), Name = name };
        ctx.Profiles.Add(profile);
        await ctx.SaveChangesAsync();
        return profile.Id;
    }
}
