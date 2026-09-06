namespace CoreGym.Domain.Services;

public record BarcodeLookupResult(
    string Barcode,
    string ProductName,
    string? ProductNameAr,
    string? Brand,
    decimal? ServingSizeG,
    decimal Calories,
    decimal ProteinG,
    decimal CarbsG,
    decimal FatG,
    string Source,
    bool FromCache);

/// <summary>
/// Replaces the lookup-barcode Edge Function's 3-tier lookup:
/// barcode_products cache → Open Food Facts → Gemini estimate,
/// caching every successful fill back into barcode_products.
/// </summary>
public interface IBarcodeLookupService
{
    /// <summary>Null when the barcode could not be resolved by any tier.</summary>
    Task<BarcodeLookupResult?> LookupAsync(string barcode, CancellationToken cancellationToken = default);
}
