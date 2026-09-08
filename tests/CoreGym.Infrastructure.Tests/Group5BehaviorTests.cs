using CoreGym.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Group 5 (chat and notifications) behavioral smoke tests — the newest,
/// least battle-tested part of the original schema per the brief.
/// </summary>
[Collection("sql-smoke")]
public class Group5BehaviorTests
{
    private readonly SqlServerSmokeFixture _fx;

    public Group5BehaviorTests(SqlServerSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Conversation_coach_id_is_a_user_id_with_defaults()
    {
        var clientId = await CreateUserAsync(_fx.Context, "g5-client");
        var coachUserId = await CreateUserAsync(_fx.Context, "g5-coach-user");

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CoachId = coachUserId,
            LastMessage = "مرحبا",
        };
        _fx.Context.Conversations.Add(conversation);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.Conversations
            .Include(c => c.Client)
            .Include(c => c.Coach)
            .AsNoTracking()
            .SingleAsync(c => c.Id == conversation.Id);

        // coach_id resolves against profiles (user id), matching the original RLS.
        Assert.Equal("g5-client", loaded.Client!.Name);
        Assert.Equal("g5-coach-user", loaded.Coach!.Name);
        Assert.Equal(0, loaded.ClientUnread);
        Assert.Equal(0, loaded.CoachUnread);
        Assert.True(loaded.IsActive);
        Assert.NotNull(loaded.LastMessageAt);
        Assert.Equal("مرحبا", loaded.LastMessage);

        // A coaches.id (not a profile id) must NOT satisfy the coach_id FK.
        await using var ctx2 = _fx.CreateContext();
        ctx2.Conversations.Add(new Conversation
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CoachId = Guid.NewGuid(),
        });
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => ctx2.SaveChangesAsync());
        Assert.Equal(547, UnwrapSqlException(ex)!.Number);
    }

    [Fact]
    public async Task Messages_type_check_defaults_and_chain()
    {
        var clientId = await CreateUserAsync(_fx.Context);
        var conversationId = await CreateConversationAsync(_fx.Context, clientId);

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = clientId,
            Content = "صوتية",
            Type = "voice",
            FileUrl = "chat-voice-notes/abc.m4a",
        };
        _fx.Context.Messages.Add(message);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.Messages
            .Include(m => m.Conversation)
            .Include(m => m.Sender)
            .AsNoTracking()
            .SingleAsync(m => m.Id == message.Id);
        Assert.NotNull(loaded.Conversation);
        Assert.Equal(clientId, loaded.Sender!.Id);
        Assert.False(loaded.IsRead);
        Assert.False(loaded.IsDeleted);

        // Omitted type falls back to the DB default 'text'.
        _fx.Context.Messages.Add(new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = clientId,
            Content = "plain",
        });
        await _fx.Context.SaveChangesAsync();
        var textMessage = await ctx.Messages.AsNoTracking()
            .SingleAsync(m => m.ConversationId == conversationId && m.Id != message.Id);
        Assert.Equal("text", textMessage.Type);

        // Undocumented type value is rejected by the CHECK constraint.
        await using var ctx2 = _fx.CreateContext();
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => ctx2.Database.ExecuteSqlRawAsync(
            "INSERT INTO messages (id, conversation_id, sender_id, content, type) VALUES ({0}, {1}, {2}, {3}, {4})",
            Guid.NewGuid(), conversationId, clientId, "sticker", "sticker"));
        Assert.Equal(547, UnwrapSqlException(ex)!.Number);
    }

    [Fact]
    public async Task Notifications_fk_context_and_coach_id_without_fk()
    {
        var clientId = await CreateUserAsync(_fx.Context);
        var conversationId = await CreateConversationAsync(_fx.Context, clientId);
        var plan = new SubscriptionPlan { Id = Guid.NewGuid(), CoachId = clientId, Name = "Gold" };
        _fx.Context.SubscriptionPlans.Add(plan);
        await _fx.Context.SaveChangesAsync();

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = clientId,
            // CHECK-constrained values from the live prod dump: 'message' | 'plan'.
            Type = "message",
            Title = "New message",
            Body = "You have a new message",
            ConversationId = conversationId,
            PlanId = plan.Id,
            // RESOLVED (open question 12): coach_id references profiles(id) with an FK,
            // so it must be a real user id — the client's own id works.
            CoachId = clientId,
        };
        _fx.Context.Notifications.Add(notification);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.Notifications
            .Include(n => n.Conversation)
            .AsNoTracking()
            .SingleAsync(n => n.Id == notification.Id);

        Assert.NotNull(loaded.Conversation);
        Assert.False(loaded.IsRead);
        Assert.NotNull(loaded.CreatedAt);
    }

    [Fact]
    public async Task NotificationPreferences_defaults_trigger_and_quiet_hours()
    {
        var userId = await CreateUserAsync(_fx.Context);

        _fx.Context.NotificationPreferences.Add(new NotificationPreference { UserId = userId });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.NotificationPreferences.AsNoTracking().SingleAsync(p => p.UserId == userId);

        Assert.True(loaded.MealRemindersEnabled);
        Assert.True(loaded.WaterRemindersEnabled);
        Assert.True(loaded.CalorieAlertsEnabled);
        Assert.True(loaded.ChatNotificationsEnabled);
        Assert.Null(loaded.QuietHoursStart);
        var updatedAtBefore = loaded.UpdatedAt!.Value;

        await using var ctx2 = _fx.CreateContext();
        var tracked = await ctx2.NotificationPreferences.SingleAsync(p => p.UserId == userId);
        tracked.QuietHoursStart = new TimeSpan(22, 0, 0);
        tracked.QuietHoursEnd = new TimeSpan(6, 30, 0);
        await ctx2.SaveChangesAsync();

        await using var ctx3 = _fx.CreateContext();
        var after = await ctx3.NotificationPreferences.AsNoTracking().SingleAsync(p => p.UserId == userId);
        Assert.Equal(new TimeSpan(22, 0, 0), after.QuietHoursStart);
        Assert.Equal(new TimeSpan(6, 30, 0), after.QuietHoursEnd);
        Assert.True(after.UpdatedAt!.Value > updatedAtBefore,
            "notification_preferences.updated_at was not restamped by the trigger");
    }

    [Fact]
    public async Task NotificationLog_data_json_check_and_defaults()
    {
        var userId = await CreateUserAsync(_fx.Context);

        var entry = new NotificationLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = "meal_reminder",
            Title = "وقت الغداء",
            Body = "Did you log lunch?",
            Data = """{"onesignal_id":"abc123","deep_link":"app://meals"}""",
        };
        _fx.Context.NotificationLogs.Add(entry);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.NotificationLogs.AsNoTracking().SingleAsync(l => l.Id == entry.Id);
        Assert.Equal("""{"onesignal_id":"abc123","deep_link":"app://meals"}""", loaded.Data);
        Assert.NotNull(loaded.SentAt);
        Assert.Null(loaded.ReadAt);

        // Invalid JSON payload is rejected by the ISJSON check constraint.
        await using var ctx2 = _fx.CreateContext();
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => ctx2.Database.ExecuteSqlRawAsync(
            "INSERT INTO notification_log (id, user_id, type, title, body, data) VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
            Guid.NewGuid(), userId, "test", "t", "b", "not-json"));
        Assert.Equal(547, UnwrapSqlException(ex)!.Number);
    }

    private static async Task<Guid> CreateUserAsync(CoreGymDbContext ctx, string name = "g5-user")
    {
        var profile = new Profile { Id = Guid.NewGuid(), Name = name };
        ctx.Profiles.Add(profile);
        await ctx.SaveChangesAsync();
        return profile.Id;
    }

    private static async Task<Guid> CreateConversationAsync(CoreGymDbContext ctx, Guid clientId)
    {
        var coachUserId = await CreateUserAsync(ctx, $"g5-coach-{Guid.NewGuid():N}");
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CoachId = coachUserId,
        };
        ctx.Conversations.Add(conversation);
        await ctx.SaveChangesAsync();
        return conversation.Id;
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
