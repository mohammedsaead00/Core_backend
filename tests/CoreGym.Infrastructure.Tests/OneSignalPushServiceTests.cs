using System.Net;
using System.Text.Json;
using CoreGym.Infrastructure.Integrations;
using Microsoft.Extensions.Options;

namespace CoreGym.Infrastructure.Tests;

public class OneSignalPushServiceTests
{
    [Fact]
    public async Task Posts_expected_payload_with_auth_and_aliases()
    {
        string? uri = null;
        string? authorization = null;
        string? body = null;
        var handler = new StubHandler(request =>
        {
            uri = request.RequestUri!.AbsoluteUri;
            authorization = request.Headers.Authorization?.ToString();
            body = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult(); // read before the service disposes it
            return """{"id":"123e4567","recipients":1}""";
        });
        var service = new OneSignalPushService(
            new HttpClient(handler),
            Options.Create(new OneSignalOptions { AppId = "app-123", RestApiKey = "rest-key" }));

        var userId = Guid.NewGuid();
        await service.SendToUsersAsync(
            new[] { userId },
            "Coach Name",
            "Voice message",
            new Dictionary<string, string> { ["conversationId"] = "abc" });

        Assert.Equal("https://onesignal.com/api/v1/notifications", uri);
        Assert.Equal("Basic rest-key", authorization);

        using var doc = JsonDocument.Parse(body!);
        Assert.Equal("app-123", doc.RootElement.GetProperty("app_id").GetString());
        Assert.Equal("push", doc.RootElement.GetProperty("target_channel").GetString());
        Assert.Equal(userId.ToString(),
            doc.RootElement.GetProperty("include_aliases").GetProperty("external_id")[0].GetString());
        Assert.Equal("Coach Name", doc.RootElement.GetProperty("headings").GetProperty("en").GetString());
        Assert.Equal("Voice message", doc.RootElement.GetProperty("contents").GetProperty("en").GetString());
        Assert.Equal("abc", doc.RootElement.GetProperty("data").GetProperty("conversationId").GetString());
    }

    [Fact]
    public async Task Unconfigured_app_id_is_a_noop()
    {
        var called = false;
        var handler = new StubHandler(_ =>
        {
            called = true;
            return "{}";
        });
        var service = new OneSignalPushService(
            new HttpClient(handler),
            Options.Create(new OneSignalOptions()));

        await service.SendToUsersAsync(new[] { Guid.NewGuid() }, "t", "b");

        Assert.False(called);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, string> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responder(request)),
            });
    }
}
