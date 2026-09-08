using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

/// <summary>Weekly chart data; the app upserts per (week, day) — no unique constraint in the schema.</summary>
[ApiController]
[Route("api/me/weekly-activity")]
[Authorize]
public class WeeklyActivityController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public WeeklyActivityController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] DateTime? weekStart, CancellationToken cancellationToken = default)
    {
        var query = _db.WeeklyActivities.AsNoTracking().Where(w => w.UserId == UserId);
        if (weekStart is not null)
        {
            query = query.Where(w => w.WeekStart == weekStart.Value.Date);
        }

        var rows = await query.OrderBy(w => w.WeekStart).ThenBy(w => w.DayIndex).ToListAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpsertWeeklyActivityRequest request, CancellationToken cancellationToken)
    {
        var weekStart = request.WeekStart.Date;
        if (request.DayIndex is < 0 or > 6)
        {
            throw new ArgumentException("DayIndex must be between 0 and 6.");
        }

        var row = await _db.WeeklyActivities
            .FirstOrDefaultAsync(w => w.UserId == UserId && w.WeekStart == weekStart && w.DayIndex == request.DayIndex, cancellationToken);
        if (row is null)
        {
            row = new WeeklyActivity
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                WeekStart = weekStart,
                DayIndex = request.DayIndex,
            };
            _db.WeeklyActivities.Add(row);
        }

        if (request.ActualPct is not null) row.ActualPct = request.ActualPct;
        if (request.GoalPct is not null) row.GoalPct = request.GoalPct;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(row);
    }
}
