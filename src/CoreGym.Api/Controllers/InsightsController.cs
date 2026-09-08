using CoreGym.Domain.Authorization;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

/// <summary>Read endpoints over the three SQL Server views (keyless EF read models).</summary>
[ApiController]
[Route("api/me")]
[Authorize]
public class InsightsController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public InsightsController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    [HttpGet("personal-records")]
    public async Task<IActionResult> PersonalRecords([FromQuery] string? exerciseName, [FromQuery] int limit = 100, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 500);
        var query = _db.PersonalRecords.AsNoTracking().Where(r => r.UserId == UserId);
        if (!string.IsNullOrWhiteSpace(exerciseName))
        {
            query = query.Where(r => r.ExerciseName == exerciseName.Trim());
        }

        var records = await query.OrderBy(r => r.ExerciseName).Take(limit).ToListAsync(cancellationToken);
        return Ok(records);
    }

    [HttpGet("weekly-progress")]
    public async Task<IActionResult> WeeklyProgress([FromQuery] int limit = 12, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 52);
        var rows = await _db.WeeklyProgress
            .AsNoTracking()
            .Where(w => w.UserId == UserId)
            .OrderByDescending(w => w.WeekStart)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpGet("weight-progress")]
    public async Task<IActionResult> WeightProgress([FromQuery] int limit = 100, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 500);
        var rows = await _db.WeightProgress
            .AsNoTracking()
            .Where(w => w.UserId == UserId)
            .OrderByDescending(w => w.MeasuredDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return Ok(rows);
    }
}
