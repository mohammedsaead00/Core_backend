namespace CoreGym.Domain.Entities;

/// <summary>
/// A client↔coach chat thread. NOTE: coach_id here is the AUTH USER id
/// (original RLS: auth.uid() = coach_id), like subscription_plans — not a
/// coaches.id.
/// </summary>
public class Conversation
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }

    public Guid CoachId { get; set; }

    /// <summary>Null when a chat starts outside an accepted-subscription flow.</summary>
    public Guid? SubscriptionId { get; set; }

    public string? LastMessage { get; set; }

    public DateTimeOffset? LastMessageAt { get; set; }

    public int ClientUnread { get; set; }

    public int CoachUnread { get; set; }

    public bool? IsActive { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? Client { get; set; }

    public Profile? Coach { get; set; }

    public Subscription? Subscription { get; set; }
}
