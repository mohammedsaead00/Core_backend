namespace CoreGym.Domain.Entities;

/// <summary>An AI voice food log (log-food-voice function output).</summary>
public class VoiceFoodLog
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? AudioPath { get; set; }

    public string? Transcript { get; set; }

    public bool? IsFood { get; set; }

    public string? Confidence { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset? LoggedAt { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? User { get; set; }
}
