namespace CoreGym.Domain.Services;

/// <summary>
/// Replaces the notify_new_message / update_conversation_on_message triggers
/// and the mark_conversation_read / unread_count RPCs. The OneSignal push side
/// of notify_new_message is deferred to Phase 3 (push integration).
/// </summary>
public interface IMessagingService
{
    /// <summary>
    /// Sends a message as the given sender (must be a conversation participant):
    /// persists it, updates the conversation preview/unread counters and creates
    /// the in-app notification for the recipient.
    /// </summary>
    Task<Entities.Message> SendMessageAsync(Guid conversationId, Guid senderId, string content, string type = "text", string? fileUrl = null, CancellationToken cancellationToken = default);

    /// <summary>Marks the other party's messages read and resets the caller's unread counter; returns the number of messages marked.</summary>
    Task<int> MarkConversationReadAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Sums unread counts across all the user's conversations.</summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
