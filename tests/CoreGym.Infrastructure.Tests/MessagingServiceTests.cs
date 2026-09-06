using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using CoreGym.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Verifies the messaging service that replaces notify_new_message /
/// update_conversation_on_message / mark_conversation_read / unread_count:
/// participant enforcement, unread counters, type-aware previews and
/// in-app notifications.
/// </summary>
[Collection("sql-smoke")]
public class MessagingServiceTests
{
    private readonly SqlServerSmokeFixture _fx;

    public MessagingServiceTests(SqlServerSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Client_send_updates_conversation_and_notifies_coach()
    {
        var (clientId, coachId, conversationId) = await CreateConversationAsync(_fx.Context);

        await using var ctx = _fx.CreateContext();
        var message = await new MessagingService(ctx)
            .SendMessageAsync(conversationId, clientId, "مرحبا");

        Assert.Equal("text", message.Type);
        await using var ctx2 = _fx.CreateContext();
        var conversation = await ctx2.Conversations.AsNoTracking().SingleAsync(c => c.Id == conversationId);
        Assert.Equal("مرحبا", conversation.LastMessage);
        Assert.Equal(1, conversation.CoachUnread);
        Assert.Equal(0, conversation.ClientUnread);
        Assert.NotNull(conversation.LastMessageAt);

        var notification = await ctx2.Notifications.AsNoTracking().SingleAsync(n => n.ConversationId == conversationId);
        Assert.Equal(coachId, notification.UserId);
        Assert.Equal("مرحبا", notification.Body);
        Assert.False(notification.IsRead);
    }

    [Fact]
    public async Task Coach_voice_send_uses_type_aware_preview_and_notifies_client()
    {
        var (clientId, coachId, conversationId) = await CreateConversationAsync(_fx.Context);

        await using var ctx = _fx.CreateContext();
        var message = await new MessagingService(ctx)
            .SendMessageAsync(conversationId, coachId, "audio-bytes", type: "voice", fileUrl: "chat-voice-notes/a.m4a");

        Assert.Equal("voice", message.Type);
        await using var ctx2 = _fx.CreateContext();
        var conversation = await ctx2.Conversations.AsNoTracking().SingleAsync(c => c.Id == conversationId);
        Assert.Equal("Voice message", conversation.LastMessage);
        Assert.Equal(1, conversation.ClientUnread);
        Assert.Equal(0, conversation.CoachUnread);

        var notification = await ctx2.Notifications.AsNoTracking().SingleAsync(n => n.ConversationId == conversationId);
        Assert.Equal(clientId, notification.UserId);
        Assert.Equal("Voice message", notification.Body);
    }

    [Fact]
    public async Task Non_participant_cannot_send_or_mark_read()
    {
        var (_, _, conversationId) = await CreateConversationAsync(_fx.Context);
        var strangerId = await CreateUserAsync(_fx.Context, "stranger");

        await using var ctx = _fx.CreateContext();
        var service = new MessagingService(ctx);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.SendMessageAsync(conversationId, strangerId, "hi"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.MarkConversationReadAsync(conversationId, strangerId));
    }

    [Fact]
    public async Task Invalid_type_and_empty_content_are_rejected()
    {
        var (_, clientId, conversationId) = await CreateConversationAsync(_fx.Context);

        await using var ctx = _fx.CreateContext();
        var service = new MessagingService(ctx);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.SendMessageAsync(conversationId, clientId, "hi", type: "sticker"));
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.SendMessageAsync(conversationId, clientId, "  "));
    }

    [Fact]
    public async Task Mark_conversation_read_marks_partner_messages_and_resets_counter()
    {
        var (clientId, coachId, conversationId) = await CreateConversationAsync(_fx.Context);

        await using (var ctx = _fx.CreateContext())
        {
            var service = new MessagingService(ctx);
            await service.SendMessageAsync(conversationId, clientId, "one");
            await service.SendMessageAsync(conversationId, clientId, "two");
        }

        await using (var ctx2 = _fx.CreateContext())
        {
            var marked = await new MessagingService(ctx2).MarkConversationReadAsync(conversationId, coachId);
            Assert.Equal(2, marked);
        }

        await using var ctx3 = _fx.CreateContext();
        var messages = await ctx3.Messages.AsNoTracking().Where(m => m.ConversationId == conversationId).ToListAsync();
        Assert.All(messages, m => Assert.True(m.IsRead));
        var conversation = await ctx3.Conversations.AsNoTracking().SingleAsync(c => c.Id == conversationId);
        Assert.Equal(0, conversation.CoachUnread);
        Assert.Equal(0, await new MessagingService(ctx3).GetUnreadCountAsync(coachId));
    }

    [Fact]
    public async Task Unread_count_sums_across_both_roles()
    {
        var (clientId, coachId, conversationId) = await CreateConversationAsync(_fx.Context);

        await using (var ctx = _fx.CreateContext())
        {
            var service = new MessagingService(ctx);
            await service.SendMessageAsync(conversationId, clientId, "one");
            await service.SendMessageAsync(conversationId, clientId, "two");
        }

        // The coach also acts as a client in a second conversation and receives a message there.
        var thirdUserId = await CreateUserAsync(_fx.Context, "third");
        await using (var ctx2 = _fx.CreateContext())
        {
            ctx2.Conversations.Add(new Conversation
            {
                Id = Guid.NewGuid(),
                ClientId = coachId,
                CoachId = thirdUserId,
            });
            await ctx2.SaveChangesAsync();
        }

        await using (var ctx3 = _fx.CreateContext())
        {
            await new MessagingService(ctx3).SendMessageAsync(
                (await ctx3.Conversations.AsNoTracking().SingleAsync(c => c.ClientId == coachId)).Id,
                thirdUserId, "hello");
        }

        await using var ctx4 = _fx.CreateContext();
        Assert.Equal(3, await new MessagingService(ctx4).GetUnreadCountAsync(coachId));
        Assert.Equal(0, await new MessagingService(ctx4).GetUnreadCountAsync(thirdUserId));
    }

    private static async Task<Guid> CreateUserAsync(CoreGymDbContext ctx, string name)
    {
        var profile = new Profile { Id = Guid.NewGuid(), Name = name };
        ctx.Profiles.Add(profile);
        await ctx.SaveChangesAsync();
        return profile.Id;
    }

    private static async Task<(Guid ClientId, Guid CoachId, Guid ConversationId)> CreateConversationAsync(CoreGymDbContext ctx)
    {
        var clientId = await CreateUserAsync(ctx, "chat-client");
        var coachId = await CreateUserAsync(ctx, "chat-coach");
        var conversation = new Conversation { Id = Guid.NewGuid(), ClientId = clientId, CoachId = coachId };
        ctx.Conversations.Add(conversation);
        await ctx.SaveChangesAsync();
        return (clientId, coachId, conversation.Id);
    }
}
