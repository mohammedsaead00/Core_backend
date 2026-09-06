namespace CoreGym.Infrastructure.Integrations;

public class OneSignalOptions
{
    public const string SectionName = "OneSignal";

    /// <summary>Null/empty disables push entirely (service becomes a no-op).</summary>
    public string? AppId { get; set; }

    public string? RestApiKey { get; set; }
}

public class StripeOptions
{
    public const string SectionName = "Stripe";

    /// <summary>Null/empty makes the webhook endpoint refuse requests (503).</summary>
    public string? WebhookSecret { get; set; }

    /// <summary>Stripe's default replay tolerance is 5 minutes.</summary>
    public int SignatureToleranceSeconds { get; set; } = 300;
}
