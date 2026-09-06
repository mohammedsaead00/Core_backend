using System.Text;
using System.Text.Json;
using CoreGym.Domain.Services;
using Microsoft.Extensions.Options;

namespace CoreGym.Infrastructure.AI;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    /// <summary>Null/empty disables the AI endpoints (they answer 503).</summary>
    public string? ApiKey { get; set; }

    /// <summary>The model the original project used (per the inventory); override as needed.</summary>
    public string Model { get; set; } = "gemini-3.6-flash";
}

/// <summary>HTTP implementation of the Gemini generateContent API.</summary>
public class GeminiClient : IGeminiClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly GeminiOptions _options;

    public GeminiClient(HttpClient http, IOptions<GeminiOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    public async Task<string> GenerateAsync(string prompt, GeminiMedia? media = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Gemini:ApiKey is not configured.");
        }

        var parts = new List<object> { new { text = prompt } };
        if (media is not null)
        {
            parts.Add(new { inline_data = new { mime_type = media.MimeType, data = media.Base64Data } });
        }

        var payload = new
        {
            contents = new[] { new { parts } },
            generation_config = new
            {
                response_mime_type = "application/json",
                temperature = 0.2,
            },
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, SerializerOptions), Encoding.UTF8, "application/json"),
        };

        using var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseJson);
        if (!doc.RootElement.TryGetProperty("candidates", out var candidates)
            || candidates.GetArrayLength() == 0
            || !candidates[0].TryGetProperty("content", out var content)
            || !content.TryGetProperty("parts", out var responseParts))
        {
            throw new InvalidOperationException("Gemini returned no candidates.");
        }

        var text = string.Concat(responseParts
            .EnumerateArray()
            .Where(p => p.TryGetProperty("text", out _))
            .Select(p => p.GetProperty("text").GetString()));
        return text;
    }
}
