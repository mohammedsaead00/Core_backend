using CoreGym.Domain.Authorization;
using CoreGym.Infrastructure;
using CoreGym.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

/// <summary>
/// Coach-facing views of a client's data. Every action authorizes against an
/// OwnedResource through the reusable OwnDataOrActiveCoach policy — the same
/// single handler used everywhere (never duplicated logic).
/// </summary>
[ApiController]
[Route("api/coach/clients/{clientUserId:guid}")]
[Authorize]
public class CoachClientsController : ControllerBase
{
    private readonly CoreGymDbContext _db;
    private readonly IAuthorizationService _authorizationService;

    public CoachClientsController(CoreGymDbContext db, IAuthorizationService authorizationService)
    {
        _db = db;
        _authorizationService = authorizationService;
    }

    private async Task<bool> CanAccessAsync(Guid clientUserId, CancellationToken cancellationToken)
    {
        var decision = await _authorizationService.AuthorizeAsync(
            User, new OwnedResource(clientUserId), AuthorizationSetup.OwnDataOrActiveCoachPolicy);
        return decision.Succeeded;
    }

    [HttpGet("summary/{date}")]
    public async Task<IActionResult> GetSummary(Guid clientUserId, DateOnly date, CancellationToken cancellationToken)
    {
        if (!await CanAccessAsync(clientUserId, cancellationToken))
        {
            return Forbid();
        }

        var summary = await _db.DailySummaries
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.UserId == clientUserId && s.SummaryDate == date.ToDateTime(TimeOnly.MinValue), cancellationToken);
        return Ok(summary);
    }

    [HttpGet("nutrition/{date}")]
    public async Task<IActionResult> GetNutrition(Guid clientUserId, DateOnly date, CancellationToken cancellationToken)
    {
        if (!await CanAccessAsync(clientUserId, cancellationToken))
        {
            return Forbid();
        }

        var logs = await _db.NutritionLogs
            .AsNoTracking()
            .Where(n => n.UserId == clientUserId && n.LoggedDate == date.ToDateTime(TimeOnly.MinValue))
            .OrderBy(n => n.LoggedAt)
            .ToListAsync(cancellationToken);
        return Ok(logs);
    }

    [HttpGet("measurements")]
    public async Task<IActionResult> GetMeasurements(Guid clientUserId, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        if (!await CanAccessAsync(clientUserId, cancellationToken))
        {
            return Forbid();
        }

        limit = Math.Clamp(limit, 1, 200);
        var measurements = await _db.BodyMeasurements
            .AsNoTracking()
            .Where(m => m.UserId == clientUserId)
            .OrderByDescending(m => m.MeasuredDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return Ok(measurements);
    }

    [HttpGet("workouts")]
    public async Task<IActionResult> GetWorkouts(
        Guid clientUserId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        if (!await CanAccessAsync(clientUserId, cancellationToken))
        {
            return Forbid();
        }

        var query = _db.WorkoutSessions.AsNoTracking().Where(s => s.UserId == clientUserId);
        if (from is not null)
        {
            query = query.Where(s => s.SessionDate >= from.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (to is not null)
        {
            query = query.Where(s => s.SessionDate <= to.Value.ToDateTime(TimeOnly.MinValue));
        }

        var sessions = await query.OrderByDescending(s => s.SessionDate).Take(100).ToListAsync(cancellationToken);
        return Ok(sessions);
    }
}
