using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

/// <summary>Records barcode scans client-side (product caching happens in the AI lookup endpoint).</summary>
[ApiController]
[Route("api/me/barcode-scans")]
[Authorize]
public class BarcodeScanHistoryController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public BarcodeScanHistoryController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 200);
        var rows = await _db.BarcodeScanHistories
            .AsNoTracking()
            .Where(h => h.UserId == UserId)
            .OrderByDescending(h => h.ScannedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RecordBarcodeScanRequest request, CancellationToken cancellationToken)
    {
        var scan = new BarcodeScanHistory
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Barcode = request.Barcode,
            QuantityG = request.QuantityG,
            NutritionLogId = request.NutritionLogId,
            ScannedAt = DateTimeOffset.UtcNow,
        };
        _db.BarcodeScanHistories.Add(scan);
        await _db.SaveChangesAsync(cancellationToken);
        return Created($"api/me/barcode-scans/{scan.Id}", scan);
    }
}
