using CoreGym.Domain.Entities;
using CoreGym.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Verifies the three Phase 1 views (inferred definitions) end-to-end through
/// EF's keyless read models: aggregation math, warmup exclusion and per-user
/// isolation.
/// </summary>
[Collection("sql-smoke")]
public class ViewTests
{
    private readonly SqlServerSmokeFixture _fx;

    public ViewTests(SqlServerSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Personal_records_bests_per_user_and_exercise_excluding_warmups()
    {
        var (userA, userB) = await CreateTwoUsersAsync(_fx.Context);

        var now = DateTimeOffset.UtcNow;
        var sets = new[]
        {
            // User A, bench: best weight 90 (5 reps), best reps 8 (80 kg); warmup must be ignored.
            new WorkoutSet { Id = Guid.NewGuid(), UserId = userA, ExerciseName = "Bench Press", SetNumber = 1, Reps = 8, WeightKg = 80m, LoggedAt = now },
            new WorkoutSet { Id = Guid.NewGuid(), UserId = userA, ExerciseName = "Bench Press", SetNumber = 2, Reps = 5, WeightKg = 90m, LoggedAt = now },
            new WorkoutSet { Id = Guid.NewGuid(), UserId = userA, ExerciseName = "Bench Press", SetNumber = 1, Reps = 10, WeightKg = 40m, IsWarmup = true, LoggedAt = now },
            // User A, squat: separate row in the view.
            new WorkoutSet { Id = Guid.NewGuid(), UserId = userA, ExerciseName = "Squat", SetNumber = 1, Reps = 3, WeightKg = 140m, LoggedAt = now },
            // User B: isolated from user A even with the same exercise.
            new WorkoutSet { Id = Guid.NewGuid(), UserId = userB, ExerciseName = "Bench Press", SetNumber = 1, Reps = 3, WeightKg = 100m, LoggedAt = now },
        };
        _fx.Context.WorkoutSets.AddRange(sets);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var records = await ctx.PersonalRecords.AsNoTracking()
            .Where(r => r.UserId == userA || r.UserId == userB)
            .ToListAsync();

        var benchA = records.Single(r => r.UserId == userA && r.ExerciseName == "Bench Press");
        Assert.Equal(90m, benchA.BestWeightKg);
        Assert.Equal(8, benchA.BestReps);
        Assert.Equal(640m, benchA.BestSetVolume);
        Assert.Equal(90m * (1m + 5m / 30m), benchA.EstimatedOneRm);
        Assert.NotNull(benchA.LastLoggedAt);

        Assert.Equal(140m, records.Single(r => r.UserId == userA && r.ExerciseName == "Squat").BestWeightKg);
        Assert.Equal(100m, records.Single(r => r.UserId == userB && r.ExerciseName == "Bench Press").BestWeightKg);
        Assert.Equal(3, records.Count(r => r.UserId == userA || r.UserId == userB));
    }

    [Fact]
    public async Task Weekly_progress_counts_logged_and_goal_met_days()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var weekStart = new DateTime(2026, 8, 31, 0, 0, 0, DateTimeKind.Utc);

        _fx.Context.WeeklyActivities.AddRange(
            new WeeklyActivity { Id = Guid.NewGuid(), UserId = userId, WeekStart = weekStart, DayIndex = 0, ActualPct = 100m, GoalPct = 100m },
            new WeeklyActivity { Id = Guid.NewGuid(), UserId = userId, WeekStart = weekStart, DayIndex = 1, ActualPct = 50m, GoalPct = 100m },
            new WeeklyActivity { Id = Guid.NewGuid(), UserId = userId, WeekStart = weekStart, DayIndex = 2, ActualPct = 80m, GoalPct = 80m });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var row = await ctx.WeeklyProgress.AsNoTracking()
            .SingleAsync(w => w.UserId == userId && w.WeekStart == weekStart);

        Assert.Equal(3, row.DaysLogged);
        Assert.Equal(2, row.DaysGoalMet);
        // SQL AVG's division rule yields a 6-decimal scale — compare rounded.
        Assert.Equal(Math.Round(230m / 3m, 2), Math.Round(row.AvgActualPct!.Value, 2));
        Assert.Equal(Math.Round(280m / 3m, 2), Math.Round(row.AvgGoalPct!.Value, 2));
    }

    [Fact]
    public async Task Weight_progress_computes_change_between_measurements()
    {
        var userId = await CreateUserAsync(_fx.Context);

        _fx.Context.BodyMeasurements.AddRange(
            new BodyMeasurement { Id = Guid.NewGuid(), UserId = userId, MeasuredDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), WeightKg = 80.0m },
            new BodyMeasurement { Id = Guid.NewGuid(), UserId = userId, MeasuredDate = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), WeightKg = 78.5m });
        // A row without a weight must be excluded entirely.
        _fx.Context.BodyMeasurements.Add(new BodyMeasurement { Id = Guid.NewGuid(), UserId = userId, MeasuredDate = new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc) });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var rows = await ctx.WeightProgress.AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.MeasuredDate)
            .ToListAsync();

        Assert.Equal(2, rows.Count);
        Assert.Equal(80.0m, rows[0].WeightKg);
        Assert.Null(rows[0].WeightChangeKg);
        Assert.Equal(78.5m, rows[1].WeightKg);
        Assert.Equal(-1.5m, rows[1].WeightChangeKg);
    }

    private static async Task<Guid> CreateUserAsync(CoreGymDbContext ctx, string name = "views-user")
    {
        var profile = new Profile { Id = Guid.NewGuid(), Name = name };
        ctx.Profiles.Add(profile);
        await ctx.SaveChangesAsync();
        return profile.Id;
    }

    private static async Task<(Guid A, Guid B)> CreateTwoUsersAsync(CoreGymDbContext ctx)
    {
        var a = await CreateUserAsync(ctx, "views-a");
        var b = await CreateUserAsync(ctx, "views-b");
        return (a, b);
    }
}
