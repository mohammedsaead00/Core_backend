using System.Text.Json;
using CoreGym.Domain.Entities;
using CoreGym.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.AI;

public class BarcodeLookupService : IBarcodeLookupService
{
    private const string OpenFoodFactsUrl = "https://world.openfoodfacts.org/api/v2/product/{0}.json";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly CoreGymDbContext _db;
    private readonly IGeminiClient _gemini;
    private readonly HttpClient _http;

    public BarcodeLookupService(CoreGymDbContext db, IGeminiClient gemini, HttpClient http)
    {
        _db = db;
        _gemini = gemini;
        _http = http;
    }

    public async Task<BarcodeLookupResult?> LookupAsync(string barcode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            throw new ArgumentException("Barcode is required.", nameof(barcode));
        }

        barcode = barcode.Trim();

        // Tier 1 — cache.
        var cached = await _db.BarcodeProducts
            .SingleOrDefaultAsync(b => b.Barcode == barcode, cancellationToken);
        if (cached is not null)
        {
            cached.LookupCount = (cached.LookupCount ?? 0) + 1;
            await _db.SaveChangesAsync(cancellationToken);
            return ToResult(cached, fromCache: true);
        }

        // Tier 2 — Open Food Facts.
        var fromOff = await TryOpenFoodFactsAsync(barcode, cancellationToken);
        if (fromOff is not null)
        {
            await CacheAsync(fromOff, cancellationToken);
            return fromOff with { FromCache = false };
        }

        // Tier 3 — Gemini estimate.
        var estimate = await TryGeminiEstimateAsync(barcode, cancellationToken);
        if (estimate is not null)
        {
            await CacheAsync(estimate, cancellationToken);
            return estimate with { FromCache = false };
        }

        return null;
    }

    private async Task<BarcodeLookupResult?> TryOpenFoodFactsAsync(string barcode, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, string.Format(OpenFoodFactsUrl, barcode));
            request.Headers.UserAgent.ParseAdd("CoreGym/1.0 (backend migration)");
            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = doc.RootElement;
            if (!root.TryGetProperty("status", out var status) || status.GetInt32() != 1
                || !root.TryGetProperty("product", out var product)
                || product.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var name = GetString(product, "product_name");
            var calories = GetNutriment(product, "energy-kcal_100g");
            if (string.IsNullOrWhiteSpace(name) || calories is null)
            {
                return null;
            }

            return new BarcodeLookupResult(
                barcode,
                name,
                GetString(product, "product_name_ar"),
                GetString(product, "brands"),
                GetNumber(product, "serving_quantity"),
                calories.Value,
                GetNutriment(product, "proteins_100g") ?? 0m,
                GetNutriment(product, "carbohydrates_100g") ?? 0m,
                GetNutriment(product, "fat_100g") ?? 0m,
                "openfoodfacts",
                FromCache: false);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task<BarcodeLookupResult?> TryGeminiEstimateAsync(string barcode, CancellationToken cancellationToken)
    {
        var prompt = $"""
            A food product has the barcode {barcode} and no Open Food Facts entry.
            Estimate its nutrition per 100 g from the barcode product category conventions (best effort).
            {ResponseContractForEstimate()}
            """;
        var raw = await _gemini.GenerateAsync(prompt, null, cancellationToken);
        using var doc = JsonDocument.Parse(ExtractJson(raw));
        var root = doc.RootElement;

        var name = GetString(root, "product_name");
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return new BarcodeLookupResult(
            barcode,
            name,
            GetString(root, "name_ar"),
            GetString(root, "brand"),
            GetDecimal(root, "serving_size_g"),
            GetDecimal(root, "calories") ?? 0m,
            GetDecimal(root, "protein_g") ?? 0m,
            GetDecimal(root, "carbs_g") ?? 0m,
            GetDecimal(root, "fat_g") ?? 0m,
            "gemini_estimate",
            FromCache: false);
    }

    private async Task CacheAsync(BarcodeLookupResult result, CancellationToken cancellationToken)
    {
        _db.BarcodeProducts.Add(new BarcodeProduct
        {
            Barcode = result.Barcode,
            ProductName = result.ProductName,
            ProductNameAr = result.ProductNameAr,
            Brand = result.Brand,
            ServingSizeG = result.ServingSizeG,
            Calories = result.Calories,
            ProteinG = result.ProteinG,
            CarbsG = result.CarbsG,
            FatG = result.FatG,
            Source = result.Source,
            Confidence = result.Source == "openfoodfacts" ? "high" : "low",
            LookupCount = 1,
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static BarcodeLookupResult ToResult(BarcodeProduct product, bool fromCache) =>
        new(product.Barcode, product.ProductName, product.ProductNameAr, product.Brand,
            product.ServingSizeG, product.Calories, product.ProteinG, product.CarbsG, product.FatG,
            product.Source, fromCache);

    private static string ResponseContractForEstimate() => """
        Respond ONLY with a JSON object in this exact shape:
        {"product_name": string, "name_ar": string|null, "brand": string|null, "serving_size_g": number|null, "calories": number, "protein_g": number, "carbs_g": number, "fat_g": number}
        """;

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static decimal? GetNumber(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        return property.GetDecimal();
    }

    private static decimal? GetNutriment(JsonElement product, string key)
    {
        if (!product.TryGetProperty("nutriments", out var nutriments) || nutriments.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return GetNumber(nutriments, key);
    }

    private static decimal? GetDecimal(JsonElement element, string propertyName) => GetNumber(element, propertyName);

    private static string ExtractJson(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        return start >= 0 && end > start ? text[start..(end + 1)] : text;
    }
}
