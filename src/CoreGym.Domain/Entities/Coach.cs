namespace CoreGym.Domain.Entities;

/// <summary>Coach record; subscriptions.coach_id and coach_content.coach_id reference this id.</summary>
public class Coach
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? Bio { get; set; }

    public decimal PriceMonthly { get; set; }

    /// <summary>Postgres text[] → JSON array.</summary>
    public List<string>? Specialization { get; set; }

    public decimal Rating { get; set; }

    public bool? IsActive { get; set; }

    public string? StripeAccountId { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Profile? User { get; set; }
}
