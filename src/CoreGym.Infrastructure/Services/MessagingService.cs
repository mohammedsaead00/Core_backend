using CoreGym.Domain.Entities;
using CoreGym.Domain.Services;
using CoreGym.Infrastructure.Integrations;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Services;

public class MessagingService : IMessagingService
{
    private readonly CoreGymDbContext _db;
    private readonly IPushNotificationService _push;

    public MessagingService(CoreGymDbContext db, IPushNotificationService? push = null)
    {
        _db = db;
        _push = push ?? new NullPushNotificationService();
    }

    public async Task<Message> SendMessageAsync(Guid conversationId, Guid senderId, string content, string type = "text", string? fileUrl = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content is required.", nameof(content));
        }

        var normalizedType = (type ?? "text").Trim().ToLowerInvariant();
        if (normalizedType is not ("text" or "voice" or "image" or "file"))
        {
            throw new ArgumentException($"Message type must be text, voice, image or file.", nameof(type));
        }

        var conversation = await _db.Conversations
            .SingleOrDefaultAsync(c => c.Id == conversationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conversation '{conversationId}' was not found.");

        if (senderId != conversation.ClientId && senderId != conversation.CoachId)
        {
            throw new UnauthorizedAccessException("Only conversation participants can send messages.");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = senderId,
            Content = content,
            Type = normalizedType,
            FileUrl = fileUrl,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _db.Messages.Add(message);

        var preview = BuildPreview(normalizedType, content);
        conversation.LastMessage = preview;
        conversation.LastMessageAt = DateTimeOffset.UtcNow;
        if (senderId == conversation.ClientId)
        {
            conversation.CoachUnread++;
        }
        else
        {
            conversation.ClientUnread++;
        }

        // In-app notification for the recipient (the OneSignal push side of
        // notify_new_message is the Phase 3 push integration).
        var recipientId = senderId == conversation.ClientId ? conversation.CoachId : conversation.ClientId;
        var senderName = await _db.Profiles
            .Where(p => p.Id == senderId)
            .Select(p => p.Name ?? p.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        _db.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = recipientId,
            // notifications.type is CHECK-constrained to 'message' | 'plan'
            // (value taken from the live prod schema dump).
            Type = "message",
            Title = string.IsNullOrWhiteSpace(senderName) ? "New message" : senderName,
            Body = preview,
            ConversationId = conversationId,
        });

        await _db.SaveChangesAsync(cancellationToken);

        // The OneSignal side of notify_new_message: best-effort — a push
        // failure must never fail the message send (already persisted above).
        try
        {
            await _push.SendToUsersAsync(
                new[] { recipientId },
                string.IsNullOrWhiteSpace(senderName) ? "New message" : senderName!,
                preview,
                new Dictionary<string, string>
                {
                    ["conversationId"] = conversationId.ToString(),
                    ["type"] = "message",
                },
                cancellationToken);
        }
        catch (Exception)
        {
            // Push delivery is retried by the client-side notification polling.
        }

        return message;
    }

    public async Task<int> MarkConversationReadAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var conversation = await _db.Conversations
            .SingleOrDefaultAsync(c => c.Id == conversationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conversation '{conversationId}' was not found.");

        if (userId != conversation.ClientId && userId != conversation.CoachId)
        {
            throw new UnauthorizedAccessException("Only conversation participants can mark it read.");
        }

        var unread = await _db.Messages
            .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
            .ToListAsync(cancellationToken);
        foreach (var message in unread)
        {
            message.IsRead = true;
        }

        if (userId == conversation.ClientId)
        {
            conversation.ClientUnread = 0;
        }
        else
        {
            conversation.CoachUnread = 0;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return unread.Count;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var asClient = await _db.Conversations
            .Where(c => c.ClientId == userId)
            .SumAsync(c => (int?)c.ClientUnread, cancellationToken) ?? 0;
        var asCoach = await _db.Conversations
            .Where(c => c.CoachId == userId)
            .SumAsync(c => (int?)c.CoachUnread, cancellationToken) ?? 0;

        return asClient + asCoach;
    }

    // Type-aware preview built by the original notify_new_message trigger.
    // The exact wording (possibly bilingual) is unconfirmed — see
    // docs/BUSINESS_LOGIC.md.
    private static string BuildPreview(string type, string content) => type switch
    {
        "voice" => "Voice message",
        "image" => "Photo",
        "file" => "File",
        _ => content,
    };
}
