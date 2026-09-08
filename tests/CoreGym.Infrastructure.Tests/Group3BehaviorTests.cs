using CoreGym.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Group 3 behavioral smoke tests: nutrition/workout/activity tables, barcode
/// cache, AI scan chains and weekly activity, against the real scratch database.
/// </summary>
[Collection("sql-smoke")]
public class Group3BehaviorTests
{
    private readonly SqlServerSmokeFixture _fx;

    public Group3BehaviorTests(SqlServerSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task NutritionLog_defaults_and_food_link()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var food = new Food { Id = Guid.NewGuid(), Name = "Chicken Breast", Calories = 165m };
        _fx.Context.Foods.Add(food);
        await _fx.Context.SaveChangesAsync();

        var log = new NutritionLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FoodId = food.Id,
            FoodName = "Chicken Breast",
            Calories = 300m,
        };
        _fx.Context.NutritionLogs.Add(log);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.NutritionLogs
            .Include(n => n.Food)
            .AsNoTracking()
            .SingleAsync(n => n.Id == log.Id);

        Assert.Equal(1m, loaded.Quantity);
        Assert.Equal("g", loaded.ServingUnit);
        Assert.Equal(0m, loaded.ProteinG);
        Assert.Equal(0m, loaded.CarbsG);
        Assert.Equal(0m, loaded.FatG);
        Assert.Equal(DateTime.UtcNow.Date, loaded.LoggedDate!.Value);
        Assert.NotNull(loaded.LoggedAt);
        Assert.Equal("Chicken Breast", loaded.Food!.Name);
    }

    [Fact]
    public async Task StreakActivityLog_source_is_constrained_to_documented_values()
    {
        var userId = await CreateUserAsync(_fx.Context);

        _fx.Context.StreakActivityLogs.Add(new StreakActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ActivityDate = DateTime.UtcNow.Date,
            Source = "workout",
        });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => ctx.Database.ExecuteSqlRawAsync(
            "INSERT INTO streak_activity_log (id, user_id, activity_date, source) VALUES ({0}, {1}, {2}, {3})",
            Guid.NewGuid(), userId, DateTime.UtcNow.Date, "meditation"));

        var sqlEx = UnwrapSqlException(ex);
        Assert.NotNull(sqlEx);
        Assert.Equal(547, sqlEx!.Number);
    }

    [Fact]
    public async Task DailyActivity_defaults()
    {
        var userId = await CreateUserAsync(_fx.Context);

        _fx.Context.DailyActivities.Add(new DailyActivity { Id = Guid.NewGuid(), UserId = userId });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.DailyActivities.AsNoTracking().SingleAsync(a => a.UserId == userId);

        Assert.Equal(0, loaded.Steps);
        Assert.Equal(0m, loaded.ActiveCaloriesBurned);
        Assert.Equal("health_connect", loaded.Source);
        Assert.Equal(DateTime.UtcNow.Date, loaded.ActivityDate!.Value);
        Assert.NotNull(loaded.SyncedAt); // NOT NULL + default in the live prod schema
    }

    [Fact]
    public async Task DailySummary_updated_at_trigger_restamps()
    {
        var userId = await CreateUserAsync(_fx.Context);

        var summary = new DailySummary { Id = Guid.NewGuid(), UserId = userId };
        _fx.Context.DailySummaries.Add(summary);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var before = (await ctx.DailySummaries.AsNoTracking().SingleAsync(s => s.Id == summary.Id))
            .UpdatedAt!.Value;

        await using var ctx2 = _fx.CreateContext();
        (await ctx2.DailySummaries.SingleAsync(s => s.Id == summary.Id)).Steps = 8000;
        await ctx2.SaveChangesAsync();

        await using var ctx3 = _fx.CreateContext();
        var after = (await ctx3.DailySummaries.AsNoTracking().SingleAsync(s => s.Id == summary.Id))
            .UpdatedAt!.Value;

        Assert.True(after > before, "daily_summary.updated_at was not restamped by the trigger");
    }

    [Fact]
    public async Task Workout_chain_fk_enforced_and_restrict_delete_blocked()
    {
        var userId = await CreateUserAsync(_fx.Context);

        var session = new WorkoutSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MuscleGroup = "chest",
            SessionName = "Push Day",
        };
        var set = new WorkoutSet
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            UserId = userId,
            ExerciseName = "Bench Press",
            SetNumber = 1,
            Reps = 8,
            WeightKg = 80m,
        };
        _fx.Context.WorkoutSessions.Add(session);
        _fx.Context.WorkoutSets.Add(set);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.WorkoutSets
            .Include(s => s.Session)
            .AsNoTracking()
            .SingleAsync(s => s.Id == set.Id);
        Assert.Equal("Push Day", loaded.Session!.SessionName);
        Assert.Equal(60, loaded.RestSec);

        // Bogus session id is rejected by the FK.
        await using var ctx2 = _fx.CreateContext();
        ctx2.WorkoutSets.Add(new WorkoutSet
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            UserId = userId,
            ExerciseName = "Row",
            SetNumber = 1,
        });
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => ctx2.SaveChangesAsync());
        Assert.Equal(547, UnwrapSqlException(ex)!.Number);

        // Deleting a session that still has sets is blocked (Restrict).
        await using var ctx3 = _fx.CreateContext();
        ctx3.WorkoutSessions.Remove(await ctx3.WorkoutSessions.SingleAsync(s => s.Id == session.Id));
        await Assert.ThrowsAnyAsync<DbUpdateException>(() => ctx3.SaveChangesAsync());
    }

    [Fact]
    public async Task BarcodeProduct_pk_source_check_and_defaults()
    {
        var product = new BarcodeProduct
        {
            Barcode = "6221031492888",
            ProductName = "Halawa",
            Source = "openfoodfacts",
        };
        _fx.Context.BarcodeProducts.Add(product);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.BarcodeProducts.AsNoTracking().SingleAsync(b => b.Barcode == product.Barcode);
        Assert.Equal(1, loaded.LookupCount);
        Assert.Equal("high", loaded.Confidence);
        Assert.Equal(0m, loaded.Calories);
        Assert.NotNull(loaded.UpdatedAt);

        // Duplicate barcode violates the primary key.
        await using var ctx2 = _fx.CreateContext();
        ctx2.BarcodeProducts.Add(new BarcodeProduct
        {
            Barcode = "6221031492888",
            ProductName = "Duplicate",
            Source = "gemini_estimate",
        });
        var dupEx = await Assert.ThrowsAnyAsync<Exception>(() => ctx2.SaveChangesAsync());
        Assert.Equal(2627, UnwrapSqlException(dupEx)!.Number);

        // Undocumented source value violates the CHECK constraint.
        await using var ctx3 = _fx.CreateContext();
        var checkEx = await Assert.ThrowsAnyAsync<Exception>(() => ctx3.Database.ExecuteSqlRawAsync(
            "INSERT INTO barcode_products (barcode, product_name, source) VALUES ({0}, {1}, {2})",
            "1234567890123", "Mystery", "chatgpt_estimate"));
        Assert.Equal(547, UnwrapSqlException(checkEx)!.Number);
    }

    [Fact]
    public async Task FoodScan_items_chain()
    {
        var userId = await CreateUserAsync(_fx.Context);

        var scan = new FoodScan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ImagePath = "food-scans/abc.jpg",
            ScannedAt = DateTimeOffset.UtcNow,
        };
        var item = new FoodScanItem
        {
            Id = Guid.NewGuid(),
            ScanId = scan.Id,
            Name = "Rice",
            NameAr = "أرز",
            EstimatedWeightG = 200m,
            Calories = 260m,
        };
        _fx.Context.FoodScans.Add(scan);
        _fx.Context.FoodScanItems.Add(item);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.FoodScanItems
            .Include(i => i.Scan)
            .AsNoTracking()
            .SingleAsync(i => i.Id == item.Id);
        Assert.Equal("food-scans/abc.jpg", loaded.Scan!.ImagePath);
        Assert.True(loaded.Scan.IsFood);
        Assert.Equal("medium", loaded.Scan.Confidence);
        Assert.Equal("أرز", loaded.NameAr);

        await using var ctx2 = _fx.CreateContext();
        ctx2.FoodScanItems.Add(new FoodScanItem
        {
            Id = Guid.NewGuid(),
            ScanId = Guid.NewGuid(),
            Name = "Orphan",
        });
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => ctx2.SaveChangesAsync());
        Assert.Equal(547, UnwrapSqlException(ex)!.Number);
    }

    [Fact]
    public async Task VoiceFoodLog_items_chain()
    {
        var userId = await CreateUserAsync(_fx.Context);

        var log = new VoiceFoodLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Transcript = "أكلت فراخ وشعر",
            LoggedAt = DateTimeOffset.UtcNow,
        };
        var item = new VoiceFoodLogItem
        {
            Id = Guid.NewGuid(),
            LogId = log.Id,
            Name = "Chicken",
            Calories = 300m,
        };
        _fx.Context.VoiceFoodLogs.Add(log);
        _fx.Context.VoiceFoodLogItems.Add(item);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.VoiceFoodLogItems
            .Include(i => i.Log)
            .AsNoTracking()
            .SingleAsync(i => i.Id == item.Id);
        Assert.Equal("أكلت فراخ وشعر", loaded.Log!.Transcript);
        Assert.Equal(0m, loaded.EstimatedWeightG);
    }

    [Fact]
    public async Task ExerciseProgress_defaults()
    {
        var userId = await CreateUserAsync(_fx.Context);

        _fx.Context.ExerciseProgress.Add(new ExerciseProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BestSetWeight = 100m,
            TotalVolume = 4200.50m,
            OneRmEstimate = 118.75m,
        });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.ExerciseProgress.AsNoTracking()
            .SingleAsync(p => p.UserId == userId);

        Assert.Equal(DateTime.UtcNow.Date, loaded.SessionDate!.Value);
        Assert.NotNull(loaded.CreatedAt);
        Assert.Null(loaded.ExerciseId);
        Assert.Null(loaded.SessionId);
    }

    [Fact]
    public async Task UserActiveProgram_defaults_and_trigger()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var program = new TrainingProgram { Id = Guid.NewGuid(), Name = "PPL" };
        _fx.Context.TrainingPrograms.Add(program);
        await _fx.Context.SaveChangesAsync();

        var active = new UserActiveProgram { Id = Guid.NewGuid(), UserId = userId, ProgramId = program.Id };
        _fx.Context.UserActivePrograms.Add(active);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.UserActivePrograms
            .Include(p => p.Program)
            .AsNoTracking()
            .SingleAsync(p => p.Id == active.Id);
        Assert.Equal(1, loaded.CurrentWeek);
        Assert.Equal(1, loaded.CurrentDay);
        Assert.NotNull(loaded.StartedAt);
        Assert.Equal("PPL", loaded.Program!.Name);
        var updatedAtBefore = loaded.UpdatedAt!.Value;

        await using var ctx2 = _fx.CreateContext();
        (await ctx2.UserActivePrograms.SingleAsync(p => p.Id == active.Id)).CurrentWeek = 2;
        await ctx2.SaveChangesAsync();

        await using var ctx3 = _fx.CreateContext();
        var after = (await ctx3.UserActivePrograms.AsNoTracking().SingleAsync(p => p.Id == active.Id))
            .UpdatedAt!.Value;
        Assert.True(after > updatedAtBefore, "user_active_program.updated_at was not restamped by the trigger");
    }

    [Fact]
    public async Task WeeklyActivity_roundtrip()
    {
        var userId = await CreateUserAsync(_fx.Context);

        _fx.Context.WeeklyActivities.Add(new WeeklyActivity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WeekStart = DateTime.UtcNow.Date,
            DayIndex = 3,
            ActualPct = 87,
        });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.WeeklyActivities.AsNoTracking()
            .SingleAsync(w => w.UserId == userId && w.DayIndex == 3);

        Assert.Equal(0, loaded.GoalPct);
        Assert.Equal(87, loaded.ActualPct);
        Assert.Equal(DateTime.UtcNow.Date, loaded.WeekStart);
    }

    private static async Task<Guid> CreateUserAsync(CoreGymDbContext ctx, string name = "g3-user")
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
