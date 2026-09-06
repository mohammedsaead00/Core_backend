using System.Net;
using System.Text;
using CoreGym.Domain.Entities;
using CoreGym.Domain.Services;
using CoreGym.Infrastructure.AI;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Verifies the lookup-barcode replacement's 3-tier flow: cache hit (with
/// lookup_count increment), Open Food Facts fill, Gemini fallback and miss.
/// </summary>
[Collection("sql-smoke")]
public class BarcodeLookupServiceTests
{
    private const string OffResponse = """
        {"status":1,"product":{"product_name":"Halawa Tehiniyeh","brands":"Al Gawhara","serving_quantity":30,
         "nutriments":{"energy-kcal_100g":486,"proteins_100g":13.2,"carbohydrates_100g":50.1,"fat_100g":27.5}}}
        """;

    [Fact]
    public async Task Cache_hit_returns_cached_product_and_increments_lookup_count()
    {
        var barcode = $"79{iota():D11}";
        await using (var ctx = _fx.CreateContext())
        {
            ctx.BarcodeProducts.Add(new BarcodeProduct
            {
                Barcode = barcode,
                ProductName = "Cached Product",
                Calories = 100m,
                Source = "openfoodfacts",
                LookupCount = 1,
            });
            await ctx.SaveChangesAsync();
        }

        var service = CreateService(handler: _ => throw new InvalidOperationException("network must not be hit on cache hit"));

        var result = await service.LookupAsync(barcode);

        Assert.NotNull(result);
        Assert.True(result.FromCache);
        Assert.Equal("Cached Product", result.ProductName);
        await using var ctx2 = _fx.CreateContext();
        Assert.Equal(2, (await ctx2.BarcodeProducts.AsNoTracking().SingleAsync(b => b.Barcode == barcode)).LookupCount);
    }

    [Fact]
    public async Task Open_food_facts_hit_is_cached_as_openfoodfacts()
    {
        var barcode = $"60{iota():D11}";
        var service = CreateService(handler: _ => OffResponse);

        var result = await service.LookupAsync(barcode);

        Assert.NotNull(result);
        Assert.Equal("openfoodfacts", result.Source);
        Assert.False(result.FromCache);
        Assert.Equal("Halawa Tehiniyeh", result.ProductName);
        Assert.Equal(486m, result.Calories);
        Assert.Equal(13.2m, result.ProteinG);

        await using var ctx = _fx.CreateContext();
        var cached = await ctx.BarcodeProducts.AsNoTracking().SingleAsync(b => b.Barcode == barcode);
        Assert.Equal("openfoodfacts", cached.Source);
        Assert.Equal("high", cached.Confidence);
    }

    [Fact]
    public async Task Gemini_estimate_is_used_when_off_has_no_product()
    {
        var barcode = $"50{iota():D11}";
        var geminiResponse = """
            {"product_name": "Mystery Bar", "name_ar": "لوح غامض", "brand": null, "serving_size_g": null,
             "calories": 480, "protein_g": 20, "carbs_g": 60, "fat_g": 18}
            """;
        var service = CreateService(
            handler: _ => """{"status":0,"product":null}""",
            geminiResponse: geminiResponse);

        var result = await service.LookupAsync(barcode);

        Assert.NotNull(result);
        Assert.Equal("gemini_estimate", result.Source);
        Assert.Equal("Mystery Bar", result.ProductName);
        Assert.Equal("لوح غامض", result.ProductNameAr);
        await using var ctx = _fx.CreateContext();
        Assert.Equal("gemini_estimate", (await ctx.BarcodeProducts.AsNoTracking().SingleAsync(b => b.Barcode == barcode)).Source);
    }

    [Fact]
    public async Task Unknown_barcode_returns_null()
    {
        var barcode = $"40{iota():D11}";
        var service = CreateService(
            handler: _ => """{"status":0}""",
            geminiResponse: """{"product_name": null}""");

        Assert.Null(await service.LookupAsync(barcode));
    }

    private static int _counter;

    private static int iota() => ++_counter;

    private BarcodeLookupService CreateService(
        Func<HttpRequestMessage, string> handler,
        string geminiResponse = "{}")
    {
        var contextFactory = new DbContextFromFixture(_fx);
        var gemini = new ScriptedGemini(geminiResponse);
        var httpClient = new HttpClient(new StubHandler(handler));
        return new BarcodeLookupService(contextFactory.Create(), gemini, httpClient);
    }

    private readonly SqlServerSmokeFixture _fx;

    public BarcodeLookupServiceTests(SqlServerSmokeFixture fx) => _fx = fx;

    private sealed class DbContextFromFixture(SqlServerSmokeFixture fx)
    {
        public CoreGymDbContext Create() => fx.CreateContext();
    }

    private sealed class ScriptedGemini(string response) : IGeminiClient
    {
        public Task<string> GenerateAsync(string prompt, GeminiMedia? media = null, CancellationToken cancellationToken = default)
            => Task.FromResult(response);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, string> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responder(request), Encoding.UTF8, "application/json"),
            });
    }
}
