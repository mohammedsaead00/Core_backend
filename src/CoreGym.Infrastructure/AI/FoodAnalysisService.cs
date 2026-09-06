using System.Text.Json;
using CoreGym.Domain.Entities;
using CoreGym.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.AI;

public class FoodAnalysisService : IFoodAnalysisService
{
    private const string ResponseContract = """
        Respond ONLY with a JSON object in this exact shape:
        {"is_food": true|false, "confidence": "high"|"medium"|"low", "transcript": string|null, "items": [{"name": string, "name_ar": string|null, "estimated_weight_g": number, "calories": number, "protein_g": number, "carbs_g": number, "fat_g": number}]}
        If no food is present, "is_food" must be false and "items" empty. Nutrition values are for the estimated portion.
        """;

    private readonly CoreGymDbContext _db;
    private readonly IGeminiClient _gemini;
    private readonly IFileStorage _storage;

    public FoodAnalysisService(CoreGymDbContext db, IGeminiClient gemini, IFileStorage storage)
    {
        _db = db;
        _gemini = gemini;
        _storage = storage;
    }

    public async Task<FoodAnalysisResult> AnalyzeImageAsync(Guid userId, string imageBase64, string? mimeType, string? notes, CancellationToken cancellationToken = default)
    {
        var prompt = "Analyze the food in this image." + NotesSuffix(notes) + ResponseContract;
        var result = Parse(await _gemini.GenerateAsync(prompt, new GeminiMedia(mimeType ?? "image/jpeg", imageBase64), cancellationToken));

        var imagePath = await _storage.SaveAsync(
            "food-scans",
            $"{Guid.NewGuid():N}{ExtensionFor(mimeType ?? "image/jpeg")}",
            new MemoryStream(DecodeBase64(imageBase64)),
            cancellationToken);

        var scan = new FoodScan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ImagePath = imagePath,
            IsFood = result.IsFood,
            Confidence = result.Confidence,
            Notes = notes,
            ScannedAt = DateTimeOffset.UtcNow,
        };
        _db.FoodScans.Add(scan);
        AddScanItems(scan.Id, result.Items);
        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<FoodAnalysisResult> AnalyzeVoiceAsync(Guid userId, string audioBase64, string? mimeType, string? notes, CancellationToken cancellationToken = default)
    {
        var prompt = "Transcribe this audio and extract every food item the speaker says they consumed." + NotesSuffix(notes) + ResponseContract;
        var result = Parse(await _gemini.GenerateAsync(prompt, new GeminiMedia(mimeType ?? "audio/mp4", audioBase64), cancellationToken));

        var audioPath = await _storage.SaveAsync(
            "voice-food-logs",
            $"{Guid.NewGuid():N}{ExtensionFor(mimeType ?? "audio/mp4")}",
            new MemoryStream(DecodeBase64(audioBase64)),
            cancellationToken);

        var log = new VoiceFoodLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AudioPath = audioPath,
            Transcript = result.Transcript,
            IsFood = result.IsFood,
            Confidence = result.Confidence,
            Notes = notes,
            LoggedAt = DateTimeOffset.UtcNow,
        };
        _db.VoiceFoodLogs.Add(log);
        foreach (var item in result.Items)
        {
            _db.VoiceFoodLogItems.Add(new VoiceFoodLogItem
            {
                Id = Guid.NewGuid(),
                LogId = log.Id,
                Name = item.Name,
                NameAr = item.NameAr,
                EstimatedWeightG = item.EstimatedWeightG,
                Calories = item.Calories,
                ProteinG = item.ProteinG,
                CarbsG = item.CarbsG,
                FatG = item.FatG,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<FoodAnalysisResult> ExtractFromTextAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Food description is required.", nameof(text));
        }

        var prompt = "Extract every food item from this description (Arabic, English or mixed)." + NotesSuffix(text) + ResponseContract;
        var result = Parse(await _gemini.GenerateAsync(prompt, null, cancellationToken));
        return result with { Transcript = text };
    }

    private void AddScanItems(Guid scanId, IReadOnlyList<DetectedFoodItem> items)
    {
        foreach (var item in items)
        {
            _db.FoodScanItems.Add(new FoodScanItem
            {
                Id = Guid.NewGuid(),
                ScanId = scanId,
                Name = item.Name,
                NameAr = item.NameAr,
                EstimatedWeightG = item.EstimatedWeightG,
                Calories = item.Calories,
                ProteinG = item.ProteinG,
                CarbsG = item.CarbsG,
                FatG = item.FatG,
            });
        }
    }

    private static FoodAnalysisResult Parse(string response)
    {
        using var doc = JsonDocument.Parse(ExtractJson(response));
        var root = doc.RootElement;

        var isFood = root.TryGetProperty("is_food", out var isFoodElement) && isFoodElement.ValueKind == JsonValueKind.True;
        var confidence = root.TryGetProperty("confidence", out var confidenceElement) && confidenceElement.ValueKind == JsonValueKind.String
            ? confidenceElement.GetString() ?? "medium"
            : "medium";
        var transcript = root.TryGetProperty("transcript", out var transcriptElement) && transcriptElement.ValueKind == JsonValueKind.String
            ? transcriptElement.GetString()
            : null;

        var items = new List<DetectedFoodItem>();
        if (root.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in itemsElement.EnumerateArray())
            {
                var name = item.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String
                    ? nameElement.GetString()
                    : null;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                items.Add(new DetectedFoodItem(
                    name,
                    item.TryGetProperty("name_ar", out var nameAr) && nameAr.ValueKind == JsonValueKind.String ? nameAr.GetString() : null,
                    item.TryGetProperty("estimated_weight_g", out var weight) && weight.ValueKind == JsonValueKind.Number ? weight.GetDecimal() : 0m,
                    item.TryGetProperty("calories", out var calories) && calories.ValueKind == JsonValueKind.Number ? calories.GetDecimal() : 0m,
                    item.TryGetProperty("protein_g", out var protein) && protein.ValueKind == JsonValueKind.Number ? protein.GetDecimal() : 0m,
                    item.TryGetProperty("carbs_g", out var carbs) && carbs.ValueKind == JsonValueKind.Number ? carbs.GetDecimal() : 0m,
                    item.TryGetProperty("fat_g", out var fat) && fat.ValueKind == JsonValueKind.Number ? fat.GetDecimal() : 0m));
            }
        }

        return new FoodAnalysisResult(isFood, confidence, transcript, items);
    }

    private static byte[] DecodeBase64(string value)
    {
        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException exception)
        {
            throw new ArgumentException("The media payload is not valid base64.", exception);
        }
    }

    private static string ExtractJson(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        return start >= 0 && end > start ? text[start..(end + 1)] : text;
    }

    private static string NotesSuffix(string? notes) =>
        string.IsNullOrWhiteSpace(notes) ? " " : $" User notes: {notes}. ";

    private static string ExtensionFor(string mimeType) => mimeType.ToLowerInvariant() switch
    {
        "image/png" => ".png",
        "image/webp" => ".webp",
        "audio/mp4" or "audio/m4a" => ".m4a",
        "audio/mpeg" => ".mp3",
        "audio/wav" => ".wav",
        _ => ".bin",
    };
}
