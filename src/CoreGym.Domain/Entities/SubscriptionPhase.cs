namespace CoreGym.Domain.Entities;

/// <summary>A phase of a coaching subscription (created/managed by the coach).</summary>
public class SubscriptionPhase
{
    public Guid Id { get; set; }

    public Guid SubscriptionId { get; set; }

    public int PhaseNumber { get; set; }

    public string Title { get; set; } = null!;

    public string? Type { get; set; }

    public string? Description { get; set; }

    public int? DurationWeeks { get; set; }

    public string? Status { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Subscription? Subscription { get; set; }
}
