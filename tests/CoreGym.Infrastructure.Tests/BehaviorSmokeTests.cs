using CoreGym.Domain.Entities;
using CoreGym.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Behavioral smoke tests: defaults, FK enforcement, JSON array columns,
/// updated_at triggers and delete behavior, executed through the real
/// SQL Server scratch database.
/// </summary>
[Collection("sql-smoke")]
public class BehaviorSmokeTests
{
    private readonly SqlServerSmokeFixture _fx;

    public BehaviorSmokeTests(SqlServerSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Profile_defaults_are_applied_by_the_database()
    {
        var profile = new Profile { Id = Guid.NewGuid() };
        _fx.Context.Profiles.Add(profile);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.Profiles.AsNoTracking().SingleAsync(p => p.Id == profile.Id);

        Assert.Equal(UserRole.Client, loaded.Role);
        Assert.Equal("", loaded.Name);
        Assert.Equal("", loaded.Email);
        Assert.NotNull(loaded.CreatedAt);
        Assert.NotNull(loaded.UpdatedAt);

        var rawRole = await ctx.Database
            .SqlQuery<string>($"SELECT role AS Value FROM profiles WHERE id = {profile.Id}")
            .SingleAsync();
        Assert.Equal("client", rawRole);
    }

    [Fact]
    public async Task UserGoals_defaults_are_applied_and_explicit_zero_is_preserved()
    {
        var userId = await CreateUserAsync(_fx.Context);

        var goals = new UserGoal { Id = Guid.NewGuid(), UserId = userId };
        _fx.Context.UserGoals.Add(goals);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.UserGoals.AsNoTracking().SingleAsync(g => g.Id == goals.Id);

        Assert.Equal(2000, loaded.DailyCalories);
        Assert.Equal(150, loaded.DailyProteinG);
        Assert.Equal(250, loaded.DailyCarbsG);
        Assert.Equal(65, loaded.DailyFatG);
        Assert.Equal(2500, loaded.DailyWaterMl);
        Assert.Equal(10000, loaded.DailySteps);
        Assert.Equal(8m, loaded.DailySleepHours);
        Assert.Equal(4, loaded.WeeklyWorkouts);

        // An explicit 0 must reach the database (not be replaced by the default 2000).
        loaded.DailyCalories = 0;
        ctx.UserGoals.Update(loaded);
        await ctx.SaveChangesAsync();

        await using var ctx2 = _fx.CreateContext();
        var reloaded = await ctx2.UserGoals.AsNoTracking().SingleAsync(g => g.Id == goals.Id);
        Assert.Equal(0, reloaded.DailyCalories);
    }

    [Fact]
    public async Task UserStreaks_one_row_per_user_with_database_defaults()
    {
        var userId = await CreateUserAsync(_fx.Context);

        _fx.Context.UserStreaks.Add(new UserStreak { UserId = userId });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.UserStreaks.AsNoTracking().SingleAsync(s => s.UserId == userId);

        Assert.Equal(0, loaded.CurrentStreak);
        Assert.Equal(0, loaded.LongestStreak);
        Assert.Equal(1, loaded.FreezeAvailable);
        Assert.Null(loaded.LastActiveDate);
    }

    [Fact]
    public async Task TrainingProgram_is_active_defaults_to_true()
    {
        var program = new TrainingProgram { Id = Guid.NewGuid(), Name = "Push Pull Legs" };
        _fx.Context.TrainingPrograms.Add(program);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.TrainingPrograms.AsNoTracking().SingleAsync(p => p.Id == program.Id);
        Assert.True(loaded.IsActive);
    }

    [Fact]
    public async Task Exercise_secondary_muscles_roundtrips_as_json_array()
    {
        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Name = "Bench Press",
            SecondaryMuscles = ["Chest", "Triceps", "أكتاف"],
        };
        _fx.Context.Exercises.Add(exercise);
        await _fx.Context.SaveChangesAsync();

        var plain = new Exercise { Id = Guid.NewGuid(), Name = "Plank", SecondaryMuscles = null };
        _fx.Context.Exercises.Add(plain);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.Exercises.AsNoTracking().SingleAsync(e => e.Id == exercise.Id);
        Assert.Equal(new List<string> { "Chest", "Triceps", "أكتاف" }, loaded.SecondaryMuscles);

        var loadedPlain = await ctx.Exercises.AsNoTracking().SingleAsync(e => e.Id == plain.Id);
        Assert.Null(loadedPlain.SecondaryMuscles);
    }

    [Fact]
    public async Task Exercise_secondary_muscles_rejects_non_json_via_check_constraint()
    {
        await using var ctx = _fx.CreateContext();
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => ctx.Database.ExecuteSqlRawAsync(
            "INSERT INTO exercises (id, name, secondary_muscles) VALUES ({0}, {1}, {2})",
            Guid.NewGuid(), "Bad Exercise", "not-json"));

        var sqlEx = UnwrapSqlException(ex);
        Assert.NotNull(sqlEx);
        Assert.Equal(547, sqlEx!.Number);
    }

    [Fact]
    public async Task Foods_defaults_and_required_calories()
    {
        var food = new Food { Id = Guid.NewGuid(), Name = "Rice", Calories = 350m };
        _fx.Context.Foods.Add(food);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.Foods.AsNoTracking().SingleAsync(f => f.Id == food.Id);

        Assert.Equal(100m, loaded.ServingSize);
        Assert.Equal("g", loaded.ServingUnit);
        Assert.Equal("other", loaded.Category);
        Assert.False(loaded.IsCustom);
        Assert.Equal(0m, loaded.ProteinG);
        Assert.Equal(0m, loaded.CarbsG);
        Assert.Equal(0m, loaded.FatG);
        Assert.Equal(0m, loaded.FiberG);

        await using var ctx2 = _fx.CreateContext();
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => ctx2.Database.ExecuteSqlRawAsync(
            "INSERT INTO foods (id, name) VALUES ({0}, {1})",
            Guid.NewGuid(), "No Calories"));

        var sqlEx = UnwrapSqlException(ex);
        Assert.NotNull(sqlEx);
        Assert.Equal(515, sqlEx!.Number);
    }

    [Fact]
    public async Task Program_chain_saves_and_bogus_program_id_is_rejected()
    {
        var program = new TrainingProgram { Id = Guid.NewGuid(), Name = "5x5" };
        var day = new ProgramDay { Id = Guid.NewGuid(), ProgramId = program.Id, DayNumber = 1, Name = "Day A" };
        var exercise = new Exercise { Id = Guid.NewGuid(), Name = "Squat" };
        var dayExercise = new ProgramDayExercise
        {
            Id = Guid.NewGuid(),
            ProgramDayId = day.Id,
            ExerciseId = exercise.Id,
            OrderIndex = 1,
            Sets = 5,
            RepsMin = 5,
            RepsMax = 5,
        };

        _fx.Context.AddRange(program, day, exercise, dayExercise);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.ProgramDayExercises
            .Include(de => de.ProgramDay)
            .ThenInclude(d => d!.Program)
            .Include(de => de.Exercise)
            .AsNoTracking()
            .SingleAsync(de => de.Id == dayExercise.Id);

        Assert.Equal("5x5", loaded.ProgramDay!.Program!.Name);
        Assert.Equal("Squat", loaded.Exercise!.Name);

        await using var ctx2 = _fx.CreateContext();
        var orphan = new ProgramDay { Id = Guid.NewGuid(), ProgramId = Guid.NewGuid(), DayNumber = 1, Name = "Orphan" };
        ctx2.ProgramDays.Add(orphan);

        var ex = await Assert.ThrowsAnyAsync<Exception>(() => ctx2.SaveChangesAsync());
        var sqlEx = UnwrapSqlException(ex);
        Assert.NotNull(sqlEx);
        Assert.Equal(547, sqlEx!.Number);
    }

    [Fact]
    public async Task Deleting_a_referenced_parent_is_blocked_by_restrict()
    {
        var program = new TrainingProgram { Id = Guid.NewGuid(), Name = "Upper Lower" };
        var day = new ProgramDay { Id = Guid.NewGuid(), ProgramId = program.Id, DayNumber = 1, Name = "Upper" };
        _fx.Context.AddRange(program, day);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var tracked = await ctx.TrainingPrograms.SingleAsync(p => p.Id == program.Id);
        ctx.TrainingPrograms.Remove(tracked);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => ctx.SaveChangesAsync());

        // Removing the child first, then the parent, succeeds.
        await using var ctx2 = _fx.CreateContext();
        var dayRow = await ctx2.ProgramDays.SingleAsync(d => d.Id == day.Id);
        var programRow = await ctx2.TrainingPrograms.SingleAsync(p => p.Id == program.Id);
        ctx2.RemoveRange(dayRow, programRow);
        await ctx2.SaveChangesAsync();

        Assert.False(await ctx2.TrainingPrograms.AnyAsync(p => p.Id == program.Id));
    }

    [Fact]
    public async Task BodyMeasurement_measured_date_defaults_to_utc_today()
    {
        var userId = await CreateUserAsync(_fx.Context);

        _fx.Context.BodyMeasurements.Add(new BodyMeasurement { Id = Guid.NewGuid(), UserId = userId });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.BodyMeasurements.AsNoTracking().SingleAsync(m => m.UserId == userId);

        Assert.Equal(DateTime.UtcNow.Date, loaded.MeasuredDate!.Value);
        Assert.NotNull(loaded.CreatedAt);
    }

    [Fact]
    public async Task UpdatedAt_triggers_stamp_profiles_and_user_goals_on_update()
    {
        var userId = await CreateUserAsync(_fx.Context, "before");

        await using var ctx = _fx.CreateContext();
        var profile = await ctx.Profiles.AsNoTracking().SingleAsync(p => p.Id == userId);
        Assert.NotNull(profile.UpdatedAt);
        var updatedAtBefore = profile.UpdatedAt!.Value;

        var goals = new UserGoal { Id = Guid.NewGuid(), UserId = userId };
        ctx.UserGoals.Add(goals);
        await ctx.SaveChangesAsync();

        var goalsBefore = (await ctx.UserGoals.AsNoTracking().SingleAsync(g => g.Id == goals.Id))
            .UpdatedAt!.Value;

        await using var ctx2 = _fx.CreateContext();
        (await ctx2.Profiles.SingleAsync(p => p.Id == userId)).Name = "after";
        (await ctx2.UserGoals.SingleAsync(g => g.Id == goals.Id)).TargetWeightKg = 75m;
        await ctx2.SaveChangesAsync();

        await using var ctx3 = _fx.CreateContext();
        var profileAfter = await ctx3.Profiles.AsNoTracking().SingleAsync(p => p.Id == userId);
        var goalsAfter = await ctx3.UserGoals.AsNoTracking().SingleAsync(g => g.Id == goals.Id);

        Assert.True(profileAfter.UpdatedAt!.Value > updatedAtBefore,
            "profiles.updated_at was not restamped by the trigger");
        Assert.True(goalsAfter.UpdatedAt!.Value > goalsBefore,
            "user_goals.updated_at was not restamped by the trigger");
    }

    private static async Task<Guid> CreateUserAsync(CoreGymDbContext ctx, string name = "test-user")
    {
        var profile = new Profile { Id = Guid.NewGuid(), Name = name };
        ctx.Profiles.Add(profile);
        await ctx.SaveChangesAsync();
        return profile.Id;
    }

    private static SqlException? UnwrapSqlException(Exception exception)
    {
        for (var current = (Exception?)exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlException)
            {
                return sqlException;
            }
        }

        return null;
    }
}
