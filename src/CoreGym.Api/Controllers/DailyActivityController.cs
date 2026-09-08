using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

/// <summary>Health Connect ingest: own daily-activity rows (no delete, per the original RLS).</summary>
[ApiController]
[Route("api/me/daily-activity")]
[Authorize]
public class DailyActivityController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public DailyActivityController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] DateOnly? date, [FromQuery] int limit = 30, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 100);
        var query = _db.DailyActivities.AsNoTracking().Where(a => a.UserId == UserId);
        if (date is not null)
        {
            query = query.Where(a => a.ActivityDate == date.Value.ToDateTime(TimeOnly.MinValue));
        }

        var rows = await query.OrderByDescending(a => a.ActivityDate).Take(limit).ToListAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDailyActivityRequest request, CancellationToken cancellationToken)
    {
        var activity = new DailyActivity
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            ActivityDate = request.Date?.Date,
            Steps = request.Steps ?? 0,
            ActiveCaloriesBurned = request.ActiveCaloriesBurned ?? 0m,
            HeartRateAvg = request.HeartRateAvg,
            ExerciseMinutes = request.ExerciseMinutes,
            Source = request.Source,
            SyncedAt = request.SyncedAt ?? DateTimeOffset.UtcNow,
        };
        _db.DailyActivities.Add(activity);
        await _db.SaveChangesAsync(cancellationToken);
        return Created($"api/me/daily-activity/{activity.Id}", activity);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDailyActivityRequest request, CancellationToken cancellationToken)
    {
        var activity = await _db.DailyActivities
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId, cancellationToken);
        if (activity is null)
        {
            return NotFound();
        }

        if (request.Steps is not null) activity.Steps = request.Steps.Value;
        if (request.ActiveCaloriesBurned is not null) activity.ActiveCaloriesBurned = request.ActiveCaloriesBurned.Value;
        if (request.HeartRateAvg is not null) activity.HeartRateAvg = request.HeartRateAvg;
        if (request.ExerciseMinutes is not null) activity.ExerciseMinutes = request.ExerciseMinutes;
        if (request.Source is not null) activity.Source = request.Source;
        if (request.SyncedAt is not null) activity.SyncedAt = request.SyncedAt;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(activity);
    }
}
