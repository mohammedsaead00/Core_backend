using CoreGym.Domain.Authorization;

namespace CoreGym.Domain.Entities;

/// <summary>A single logged food entry for a user's day.</summary>
public class NutritionLog : IOwnedResource
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>Null when the food was entered by name without a catalog entry.</summary>
    public Guid? FoodId { get; set; }

    public string FoodName { get; set; } = null!;

    public string? MealType { get; set; }

    public decimal? Quantity { get; set; }

    public string? ServingUnit { get; set; }

    public decimal Calories { get; set; }

    public decimal ProteinG { get; set; }

    public decimal CarbsG { get; set; }

    public decimal FatG { get; set; }

    public DateTime? LoggedDate { get; set; }

    public DateTimeOffset? LoggedAt { get; set; }

    public Profile? User { get; set; }

    public Food? Food { get; set; }
}
