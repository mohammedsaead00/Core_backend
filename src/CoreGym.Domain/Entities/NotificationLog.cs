namespace CoreGym.Domain.Entities;

/// <summary>
/// Every push sent through the notification service is logged here. INSERT is
/// server-side only in the original (service role); clients read/update own.
/// </summary>
public class NotificationLog
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Type { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Body { get; set; } = null!;

    /// <summary>Postgres jsonb → raw JSON string with an ISJSON check constraint.</summary>
    public string? Data { get; set; }

    public DateTimeOffset? SentAt { get; set; }

    public DateTimeOffset? ReadAt { get; set; }

    public Profile? User { get; set; }
}
