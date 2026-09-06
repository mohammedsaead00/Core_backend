using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/me/measurements")]
[Authorize]
public class MeasurementsController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public MeasurementsController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 200);
        var measurements = await _db.BodyMeasurements
            .AsNoTracking()
            .Where(m => m.UserId == UserId)
            .OrderByDescending(m => m.MeasuredDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return Ok(measurements);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMeasurementRequest request, CancellationToken cancellationToken)
    {
        var measurement = new BodyMeasurement
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            WeightKg = request.WeightKg,
            BodyFatPct = request.BodyFatPct,
            MuscleMass = request.MuscleMass,
            ChestCm = request.ChestCm,
            WaistCm = request.WaistCm,
            HipsCm = request.HipsCm,
            ArmsCm = request.ArmsCm,
            ThighsCm = request.ThighsCm,
            MeasuredDate = request.MeasuredDate?.Date,
            Notes = request.Notes,
        };
        _db.BodyMeasurements.Add(measurement);
        await _db.SaveChangesAsync(cancellationToken);
        return Created($"api/me/measurements/{measurement.Id}", measurement);
    }
}
