namespace CoreGym.Domain.Entities;

/// <summary>History of a user's barcode scans, optionally linked to the resulting nutrition log.</summary>
public class BarcodeScanHistory
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>Nullable — the original has no FK here; a scan may predate the product cache entry.</summary>
    public string? Barcode { get; set; }

    public decimal? QuantityG { get; set; }

    public Guid? NutritionLogId { get; set; }

    public DateTimeOffset? ScannedAt { get; set; }

    public Profile? User { get; set; }

    public NutritionLog? NutritionLog { get; set; }
}
