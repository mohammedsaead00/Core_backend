namespace CoreGym.Domain.Entities;

/// <summary>
/// In-app notification. Original RLS allowed client SELECT only — writes
/// happen server-side (chat trigger / notification functions, later phase).
/// </summary>
public class Notification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Type { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Body { get; set; } = null!;

    public Guid? ConversationId { get; set; }

    public Guid? PlanId { get; set; }

    /// <summary>
    /// Soft reference, deliberately NO foreign key: the original never reveals
    /// whether this holds a user id or a coaches.id (chat flow suggests user
    /// id). Confirm during the chat-phase port, then add the FK.
    /// </summary>
    public Guid? CoachId { get; set; }

    public bool IsRead { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? User { get; set; }

    public Conversation? Conversation { get; set; }

    public SubscriptionPlan? Plan { get; set; }
}
