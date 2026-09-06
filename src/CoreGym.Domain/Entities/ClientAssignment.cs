namespace CoreGym.Domain.Entities;

/// <summary>A coach assigning a piece of content to a client.</summary>
public class ClientAssignment
{
    public Guid Id { get; set; }

    public Guid CoachId { get; set; }

    public Guid ClientId { get; set; }

    public Guid ContentId { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset? AssignedAt { get; set; }

    public Coach? Coach { get; set; }

    public Profile? Client { get; set; }

    public CoachContent? Content { get; set; }
}
