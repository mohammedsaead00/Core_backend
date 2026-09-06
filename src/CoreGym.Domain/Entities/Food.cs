namespace CoreGym.Domain.Entities;

/// <summary>Food reference data; public read, custom foods are user-created.</summary>
public class Food
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? NameAr { get; set; }

    public decimal Calories { get; set; }

    public decimal ProteinG { get; set; }

    public decimal CarbsG { get; set; }

    public decimal FatG { get; set; }

    public decimal FiberG { get; set; }

    public decimal? ServingSize { get; set; }

    public string? ServingUnit { get; set; }

    public bool IsCustom { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public string? Category { get; set; }

    public string? ImageUrl { get; set; }

    public Profile? Creator { get; set; }
}
