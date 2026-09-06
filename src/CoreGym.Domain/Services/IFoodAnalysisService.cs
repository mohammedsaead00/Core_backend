namespace CoreGym.Domain.Services;

public record DetectedFoodItem(
    string Name,
    string? NameAr,
    decimal EstimatedWeightG,
    decimal Calories,
    decimal ProteinG,
    decimal CarbsG,
    decimal FatG);

public record FoodAnalysisResult(
    bool IsFood,
    string Confidence,
    string? Transcript,
    IReadOnlyList<DetectedFoodItem> Items);

/// <summary>
/// Replaces the analyze-food / log-food-voice / log-food-text Edge Functions:
/// Gemini extracts structured food items; image/voice runs persist a scan and
/// its items (text is stateless — the client confirms and logs normally).
/// </summary>
public interface IFoodAnalysisService
{
    Task<FoodAnalysisResult> AnalyzeImageAsync(Guid userId, string imageBase64, string? mimeType, string? notes, CancellationToken cancellationToken = default);

    Task<FoodAnalysisResult> AnalyzeVoiceAsync(Guid userId, string audioBase64, string? mimeType, string? notes, CancellationToken cancellationToken = default);

    Task<FoodAnalysisResult> ExtractFromTextAsync(string text, CancellationToken cancellationToken = default);
}
