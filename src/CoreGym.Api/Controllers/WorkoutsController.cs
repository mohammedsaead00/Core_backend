using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Domain.Services;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/workouts")]
[Authorize]
public class WorkoutsController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;
    private readonly IDailySummaryService _summary;

    public WorkoutsController(CoreGymDbContext db, IDailySummaryService summary, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
        _summary = summary;
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> ListSessions(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 200);
        var query = _db.WorkoutSessions.AsNoTracking().Where(s => s.UserId == UserId);
        if (from is not null)
        {
            query = query.Where(s => s.SessionDate >= from.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (to is not null)
        {
            query = query.Where(s => s.SessionDate <= to.Value.ToDateTime(TimeOnly.MinValue));
        }

        var sessions = await query.OrderByDescending(s => s.SessionDate).Take(limit).ToListAsync(cancellationToken);
        return Ok(sessions);
    }

    /// <summary>Creates a session with its sets and re-syncs the daily summary, replacing the sync trigger.</summary>
    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateWorkoutSessionRequest request, CancellationToken cancellationToken)
    {
        var session = new WorkoutSession
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            MuscleGroup = request.MuscleGroup,
            SessionName = request.SessionName,
            DurationMin = request.DurationMin,
            Notes = request.Notes,
            SessionDate = request.Date?.Date,
            StartedAt = request.StartedAt,
            EndedAt = request.EndedAt,
        };
        _db.WorkoutSessions.Add(session);

        if (request.Sets is not null)
        {
            foreach (var set in request.Sets)
            {
                _db.WorkoutSets.Add(new WorkoutSet
                {
                    Id = Guid.NewGuid(),
                    SessionId = session.Id,
                    UserId = UserId,
                    ExerciseName = set.ExerciseName,
                    SetNumber = set.SetNumber,
                    Reps = set.Reps,
                    WeightKg = set.WeightKg,
                    DurationSec = set.DurationSec,
                    RestSec = set.RestSec,
                    IsWarmup = set.IsWarmup,
                    LoggedAt = DateTimeOffset.UtcNow,
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _summary.SyncWorkoutAsync(UserId, session.SessionDate ?? DateTime.UtcNow.Date, cancellationToken);
        return Created($"api/workouts/sessions/{session.Id}", new { session, sets = request.Sets });
    }

    /// <summary>Deletes a session and its sets (sets first — the FK is Restrict), then re-syncs the summary.</summary>
    [HttpDelete("sessions/{id:guid}")]
    public async Task<IActionResult> DeleteSession(Guid id, CancellationToken cancellationToken)
    {
        var session = await _db.WorkoutSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId, cancellationToken);
        if (session is null)
        {
            return NotFound();
        }

        var sets = await _db.WorkoutSets.Where(w => w.SessionId == id).ToListAsync(cancellationToken);
        _db.WorkoutSets.RemoveRange(sets);
        _db.WorkoutSessions.Remove(session);
        await _db.SaveChangesAsync(cancellationToken);

        await _summary.SyncWorkoutAsync(UserId, session.SessionDate ?? DateTime.UtcNow.Date, cancellationToken);
        return NoContent();
    }

    [HttpGet("sets")]
    public async Task<IActionResult> ListSets([FromQuery] Guid sessionId, CancellationToken cancellationToken)
    {
        var sets = await _db.WorkoutSets
            .AsNoTracking()
            .Where(w => w.SessionId == sessionId && w.UserId == UserId)
            .OrderBy(w => w.SetNumber)
            .ToListAsync(cancellationToken);
        return Ok(sets);
    }
}
