using CoreGym.Domain.Authorization;

namespace CoreGym.Domain.Entities;

/// <summary>Periodic body measurements per user.</summary>
public class BodyMeasurement : IOwnedResource
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public decimal? WeightKg { get; set; }

    public decimal? BodyFatPct { get; set; }

    public decimal? MuscleMass { get; set; }

    public decimal? ChestCm { get; set; }

    public decimal? WaistCm { get; set; }

    public decimal? HipsCm { get; set; }

    public decimal? ArmsCm { get; set; }

    public decimal? ThighsCm { get; set; }

    public DateTime? MeasuredDate { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public Profile? User { get; set; }
}
