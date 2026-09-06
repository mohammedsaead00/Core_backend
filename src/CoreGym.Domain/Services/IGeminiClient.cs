namespace CoreGym.Domain.Services;

/// <summary>
/// Thin abstraction over the Gemini generateContent API so the food-analysis
/// services are testable without network access.
/// </summary>
public interface IGeminiClient
{
    /// <summary>Sends the prompt (plus optional inline media) and returns the model's text response.</summary>
    Task<string> GenerateAsync(string prompt, GeminiMedia? media = null, CancellationToken cancellationToken = default);
}

public record GeminiMedia(string MimeType, string Base64Data);
