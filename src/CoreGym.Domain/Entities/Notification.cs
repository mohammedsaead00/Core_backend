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

    /// <summary>No FK in the original schema; references subscription_plans.id by convention only.</summary>
    public Guid? PlanId { get; set; }

    /// <summary>
    /// RESOLVED via the live prod schema dump (2026-09-07): references
    /// profiles.id (a user id) and carries a foreign key.
    /// </summary>
    public Guid? CoachId { get; set; }

    public bool IsRead { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? User { get; set; }

    public Conversation? Conversation { get; set; }

    /// <summary>The coach user (coach_id = profiles.id per the prod dump).</summary>
    public Profile? Coach { get; set; }
}
