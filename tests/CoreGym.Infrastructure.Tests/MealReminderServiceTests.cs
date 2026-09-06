using CoreGym.Domain.Entities;
using CoreGym.Domain.Services;
using CoreGym.Infrastructure.Notifications;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Verifies the send-meal-reminders replacement: per-window dedupe via
/// notification_log, Cairo-timezone quiet hours and already-logged skip,
/// and push-failure retry semantics.
/// </summary>
[Collection("sql-smoke")]
public class MealReminderServiceTests
{
    // 07:00 UTC = 10:00 Cairo (UTC+3 in September) — inside the morning window (06–12 UTC).
    private static readonly DateTimeOffset Now = new(2026, 9, 7, 7, 0, 0, TimeSpan.Zero);

    private readonly SqlServerSmokeFixture _fx;

    public MealReminderServiceTests(SqlServerSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Sends_and_logs_for_a_user_who_has_not_logged_today()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var push = new CapturingPush();

        await using var ctx = _fx.CreateContext();
        var summary = await new MealReminderService(ctx, push).RunAsync(Now);

        Assert.Equal(1, summary.ProfilesChecked);
        Assert.Equal(1, summary.RemindersSent);
        var sent = Assert.Single(push.Sent);
        Assert.Equal(userId, sent.userId);
        Assert.Equal(MealReminderService.Title, sent.title);

        await using var ctx2 = _fx.CreateContext();
        var log = await ctx2.NotificationLogs.AsNoTracking().SingleAsync(l => l.UserId == userId);
        Assert.Equal("meal_reminder", log.Type);
        Assert.Equal(Now.UtcDateTime, log.SentAt!.Value.UtcDateTime);
    }

    [Fact]
    public async Task Skips_users_who_already_logged_food_today_cairo()
    {
        var userId = await CreateUserAsync(_fx.Context);
        await using (var ctx = _fx.CreateContext())
        {
            ctx.NutritionLogs.Add(new NutritionLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FoodName = "Breakfast",
                Calories = 400m,
                LoggedDate = new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc), // Cairo date of `Now`
            });
            await ctx.SaveChangesAsync();
        }

        await using var ctx2 = _fx.CreateContext();
        var summary = await new MealReminderService(ctx2, new CapturingPush()).RunAsync(Now);

        Assert.Equal(1, summary.SkippedAlreadyLogged);
        Assert.Equal(0, summary.RemindersSent);
    }

    [Fact]
    public async Task Second_run_in_the_same_window_is_skipped()
    {
        var userId = await CreateUserAsync(_fx.Context);

        await using (var ctx = _fx.CreateContext())
        {
            await new MealReminderService(ctx, new CapturingPush()).RunAsync(Now);
        }

        await using var ctx2 = _fx.CreateContext();
        var summary = await new MealReminderService(ctx2, new CapturingPush()).RunAsync(Now);

        Assert.Equal(1, summary.SkippedAlreadyReminded);
        Assert.Equal(0, summary.RemindersSent);
        Assert.Equal(1, await ctx2.NotificationLogs.CountAsync(l => l.UserId == userId));
    }

    [Fact]
    public async Task Quiet_hours_are_evaluated_in_cairo_local_time()
    {
        var userId = await CreateUserAsync(_fx.Context);
        await using (var ctx = _fx.CreateContext())
        {
            // 10:00 Cairo falls inside 08:00–23:00 quiet hours.
            ctx.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = userId,
                QuietHoursStart = new TimeSpan(8, 0, 0),
                QuietHoursEnd = new TimeSpan(23, 0, 0),
            });
            await ctx.SaveChangesAsync();
        }

        await using var ctx2 = _fx.CreateContext();
        var summary = await new MealReminderService(ctx2, new CapturingPush()).RunAsync(Now);

        Assert.Equal(1, summary.SkippedQuietHours);
        Assert.Equal(0, summary.RemindersSent);
    }

    [Fact]
    public async Task Disabled_reminders_are_skipped()
    {
        var userId = await CreateUserAsync(_fx.Context);
        await using (var ctx = _fx.CreateContext())
        {
            ctx.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = userId,
                MealRemindersEnabled = false,
            });
            await ctx.SaveChangesAsync();
        }

        await using var ctx2 = _fx.CreateContext();
        var summary = await new MealReminderService(ctx2, new CapturingPush()).RunAsync(Now);

        Assert.Equal(1, summary.SkippedDisabled);
        Assert.Equal(0, summary.RemindersSent);
    }

    [Fact]
    public async Task Push_failure_does_not_log_so_the_window_retries()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var failingPush = new CapturingPush(throwOnSend: true);

        await using var ctx = _fx.CreateContext();
        var summary = await new MealReminderService(ctx, failingPush).RunAsync(Now);

        Assert.Equal(0, summary.RemindersSent);
        await using var ctx2 = _fx.CreateContext();
        Assert.False(await ctx2.NotificationLogs.AnyAsync(l => l.UserId == userId));
    }

    [Fact]
    public async Task Outside_all_windows_nothing_runs()
    {
        var userId = await CreateUserAsync(_fx.Context);
        await using var ctx = _fx.CreateContext();

        var summary = await new MealReminderService(ctx, new CapturingPush()).RunAsync(Now.AddHours(-2)); // 04:00 UTC

        Assert.Equal(0, summary.ProfilesChecked);
    }

    private static async Task<Guid> CreateUserAsync(CoreGymDbContext ctx)
    {
        var profile = new Profile { Id = Guid.NewGuid(), Name = "reminder-user" };
        ctx.Profiles.Add(profile);
        await ctx.SaveChangesAsync();
        return profile.Id;
    }

    private sealed class CapturingPush(bool throwOnSend = false) : IPushNotificationService
    {
        public List<(Guid userId, string title, string body)> Sent { get; } = [];

        public Task SendToUsersAsync(IReadOnlyCollection<Guid> userIds, string title, string body,
            IReadOnlyDictionary<string, string>? data = null, CancellationToken cancellationToken = default)
        {
            if (throwOnSend)
            {
                throw new HttpRequestException("onesignal down");
            }

            foreach (var userId in userIds)
            {
                Sent.Add((userId, title, body));
            }

            return Task.CompletedTask;
        }
    }
}
