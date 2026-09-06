namespace CoreGym.Domain.Entities;

/// <summary>A chat message inside a conversation.</summary>
public class Message
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string Content { get; set; } = null!;

    /// <summary>'text' | 'voice' | 'image' | 'file' — enforced by a CHECK constraint.</summary>
    public string? Type { get; set; }

    public string? FileUrl { get; set; }

    public bool IsRead { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Conversation? Conversation { get; set; }

    public Profile? Sender { get; set; }
}
