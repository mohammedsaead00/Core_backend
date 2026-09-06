using System.Text.Json;
using CoreGym.Domain.Entities;
using CoreGym.Domain.Services;
using CoreGym.Infrastructure.AI;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Verifies the analyze-food/log-food-voice replacements with a scripted
/// Gemini client: JSON parsing (incl. markdown fences), persistence of scans
/// and items, and storage of the media file.
/// </summary>
[Collection("sql-smoke")]
public class FoodAnalysisServiceTests
{
    private readonly SqlServerSmokeFixture _fx;

    public FoodAnalysisServiceTests(SqlServerSmokeFixture fx) => _fx = fx;

    private const string GoodResponse = """
        ```json
        {"is_food": true, "confidence": "high", "transcript": null,
         "items": [{"name": "Grilled Chicken", "name_ar": "دجاج مشوي", "estimated_weight_g": 200, "calories": 330, "protein_g": 62, "carbs_g": 0, "fat_g": 7}]}
        ```
        """;

    [Fact]
    public async Task AnalyzeImage_persists_scan_items_and_media_file()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var gemini = new ScriptedGeminiClient(GoodResponse);
        var storage = new InMemoryFileStorage();
        var imageBase64 = Convert.ToBase64String([1, 2, 3, 4, 5]);

        await using var ctx = _fx.CreateContext();
        var result = await new FoodAnalysisService(ctx, gemini, storage)
            .AnalyzeImageAsync(userId, imageBase64, "image/jpeg", "with rice", default);

        Assert.True(result.IsFood);
        Assert.Equal("high", result.Confidence);
        var item = Assert.Single(result.Items);
        Assert.Equal("Grilled Chicken", item.Name);
        Assert.Equal("دجاج مشوي", item.NameAr);
        Assert.Equal(330m, item.Calories);

        await using var ctx2 = _fx.CreateContext();
        var scan = await ctx2.FoodScans.AsNoTracking().SingleAsync(s => s.UserId == userId);
        Assert.True(scan.IsFood);
        Assert.NotNull(scan.ImagePath);
        Assert.StartsWith("food-scans/", scan.ImagePath);
        Assert.True(storage.Contains(scan.ImagePath!));

        var savedItem = await ctx2.FoodScanItems.AsNoTracking().SingleAsync(i => i.ScanId == scan.Id);
        Assert.Equal("Grilled Chicken", savedItem.Name);
        Assert.Equal(200m, savedItem.EstimatedWeightG);
    }

    [Fact]
    public async Task AnalyzeVoice_persists_log_with_transcript()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var response = """
            {"is_food": true, "confidence": "medium", "transcript": "أكلت فرخ وشعر",
             "items": [{"name": "Chicken", "name_ar": "فراخ", "estimated_weight_g": 150, "calories": 250, "protein_g": 45, "carbs_g": 0, "fat_g": 5}]}
            """;
        var storage = new InMemoryFileStorage();

        await using var ctx = _fx.CreateContext();
        var result = await new FoodAnalysisService(ctx, new ScriptedGeminiClient(response), storage)
            .AnalyzeVoiceAsync(userId, Convert.ToBase64String([9, 9, 9]), "audio/mp4", null, default);

        Assert.Equal("أكلت فرخ وشعر", result.Transcript);

        await using var ctx2 = _fx.CreateContext();
        var log = await ctx2.VoiceFoodLogs.AsNoTracking().SingleAsync(l => l.UserId == userId);
        Assert.Equal("أكلت فرخ وشعر", log.Transcript);
        Assert.StartsWith("voice-food-logs/", log.AudioPath);
        Assert.True(storage.Contains(log.AudioPath!));
        Assert.Equal(250m, (await ctx2.VoiceFoodLogItems.AsNoTracking().SingleAsync(i => i.LogId == log.Id)).Calories);
    }

    [Fact]
    public async Task ExtractFromText_is_stateless()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var storage = new InMemoryFileStorage();
        await using var ctx = _fx.CreateContext();

        var result = await new FoodAnalysisService(ctx, new ScriptedGeminiClient(GoodResponse), storage)
            .ExtractFromTextAsync("2 eggs and toast", default);

        Assert.True(result.IsFood);
        Assert.Equal("2 eggs and toast", result.Transcript);
        Assert.Empty(storage.AllPaths());
        Assert.False(await ctx.FoodScans.AnyAsync(s => s.UserId == userId));
        Assert.False(await ctx.VoiceFoodLogs.AnyAsync(l => l.UserId == userId));
    }

    [Fact]
    public async Task Non_food_image_persists_scan_with_no_items()
    {
        var userId = await CreateUserAsync(_fx.Context);
        var response = """{"is_food": false, "confidence": "high", "transcript": null, "items": []}""";
        await using var ctx = _fx.CreateContext();

        var result = await new FoodAnalysisService(ctx, new ScriptedGeminiClient(response), new InMemoryFileStorage())
            .AnalyzeImageAsync(userId, Convert.ToBase64String([7]), "image/png", null, default);

        Assert.False(result.IsFood);
        Assert.Empty(result.Items);
        await using var ctx2 = _fx.CreateContext();
        var scan = await ctx2.FoodScans.AsNoTracking().SingleAsync(s => s.UserId == userId);
        Assert.False(await ctx2.FoodScanItems.AnyAsync(i => i.ScanId == scan.Id));
    }

    [Fact]
    public async Task Invalid_base64_is_rejected_as_bad_request()
    {
        var userId = await CreateUserAsync(_fx.Context);
        await using var ctx = _fx.CreateContext();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            new FoodAnalysisService(ctx, new ScriptedGeminiClient(GoodResponse), new InMemoryFileStorage())
                .AnalyzeImageAsync(userId, "not-base64!!!", null, null, default));
    }

    private static async Task<Guid> CreateUserAsync(CoreGymDbContext ctx)
    {
        var profile = new Profile { Id = Guid.NewGuid(), Name = "ai-user" };
        ctx.Profiles.Add(profile);
        await ctx.SaveChangesAsync();
        return profile.Id;
    }

    private sealed class ScriptedGeminiClient(string response) : IGeminiClient
    {
        public Task<string> GenerateAsync(string prompt, GeminiMedia? media = null, CancellationToken cancellationToken = default)
            => Task.FromResult(response);
    }

    private sealed class InMemoryFileStorage : IFileStorage
    {
        private readonly Dictionary<string, byte[]> _files = new();

        public Task<string> SaveAsync(string bucket, string fileName, Stream content, CancellationToken cancellationToken = default)
        {
            using var memory = new MemoryStream();
            content.CopyTo(memory);
            _files[$"{bucket}/{fileName}"] = memory.ToArray();
            return Task.FromResult($"{bucket}/{fileName}");
        }

        public Task<Stream?> OpenAsync(string bucket, string fileName, CancellationToken cancellationToken = default)
            => Task.FromResult<Stream?>(_files.TryGetValue($"{bucket}/{fileName}", out var bytes) ? new MemoryStream(bytes) : null);

        public Task DeleteAsync(string bucket, string fileName, CancellationToken cancellationToken = default)
        {
            _files.Remove($"{bucket}/{fileName}");
            return Task.CompletedTask;
        }

        public IReadOnlyCollection<string> AllPaths() => _files.Keys;

        public bool Contains(string path) => _files.ContainsKey(path);
    }
}
