namespace CoreGym.Domain.Entities;

/// <summary>History of a user's barcode scans, optionally linked to the resulting nutrition log.</summary>
public class BarcodeScanHistory
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Barcode { get; set; } = null!;

    public decimal? QuantityG { get; set; }

    public Guid? NutritionLogId { get; set; }

    public DateTimeOffset? ScannedAt { get; set; }

    public Profile? User { get; set; }

    public BarcodeProduct? Product { get; set; }

    public NutritionLog? NutritionLog { get; set; }
}
