namespace CoreGym.Domain.Entities;

/// <summary>A food item extracted from a VoiceFoodLog.</summary>
public class VoiceFoodLogItem
{
    public Guid Id { get; set; }

    public Guid LogId { get; set; }

    public string Name { get; set; } = null!;

    public string? NameAr { get; set; }

    public decimal EstimatedWeightG { get; set; }

    public decimal Calories { get; set; }

    public decimal ProteinG { get; set; }

    public decimal CarbsG { get; set; }

    public decimal FatG { get; set; }

    public Guid? NutritionLogId { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public VoiceFoodLog? Log { get; set; }

    public NutritionLog? NutritionLog { get; set; }
}
