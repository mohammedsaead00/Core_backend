namespace CoreGym.Domain.Entities;

/// <summary>
/// Shared barcode → nutrition cache (public read; writes only from the
/// lookup-barcode function replacement in a later phase).
/// </summary>
public class BarcodeProduct
{
    /// <summary>The barcode string itself is the primary key.</summary>
    public string Barcode { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string? ProductNameAr { get; set; }

    public string? Brand { get; set; }

    public decimal? ServingSizeG { get; set; }

    public decimal Calories { get; set; }

    public decimal ProteinG { get; set; }

    public decimal CarbsG { get; set; }

    public decimal FatG { get; set; }

    /// <summary>'openfoodfacts' or 'gemini_estimate' — enforced by a CHECK constraint.</summary>
    public string Source { get; set; } = null!;

    public string? Confidence { get; set; }

    public int? LookupCount { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
