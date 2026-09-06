using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CoreGym.Domain.Services;
using Microsoft.Extensions.Options;

namespace CoreGym.Infrastructure.Integrations;

public class OneSignalPushService : IPushNotificationService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly OneSignalOptions _options;

    public OneSignalPushService(HttpClient http, IOptions<OneSignalOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task SendToUsersAsync(
        IReadOnlyCollection<Guid> userIds,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        // Push is optional: without configuration the service is a no-op.
        if (string.IsNullOrWhiteSpace(_options.AppId) || userIds.Count == 0)
        {
            return;
        }

        var payload = new
        {
            app_id = _options.AppId,
            include_aliases = new { external_id = userIds.Select(u => u.ToString()).ToArray() },
            target_channel = "push",
            headings = new { en = title, ar = title },
            contents = new { en = body, ar = body },
            data,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://onesignal.com/api/v1/notifications")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, SerializerOptions), Encoding.UTF8, "application/json"),
        };
        if (!string.IsNullOrWhiteSpace(_options.RestApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _options.RestApiKey);
        }

        using var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

/// <summary>Push sink used in tests and wherever push is not configured.</summary>
public sealed class NullPushNotificationService : IPushNotificationService
{
    public Task SendToUsersAsync(
        IReadOnlyCollection<Guid> userIds,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
