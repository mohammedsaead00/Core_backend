using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/me/exercise-progress")]
[Authorize]
public class ExerciseProgressController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public ExerciseProgressController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? sessionId,
        [FromQuery] Guid? exerciseId,
        [FromQuery] DateOnly? date,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 200);
        var query = _db.ExerciseProgress.AsNoTracking().Where(p => p.UserId == UserId);
        if (sessionId is not null)
        {
            query = query.Where(p => p.SessionId == sessionId);
        }

        if (exerciseId is not null)
        {
            query = query.Where(p => p.ExerciseId == exerciseId);
        }

        if (date is not null)
        {
            query = query.Where(p => p.SessionDate == date.Value.ToDateTime(TimeOnly.MinValue));
        }

        var rows = await query.OrderByDescending(p => p.SessionDate).Take(limit).ToListAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExerciseProgressRequest request, CancellationToken cancellationToken)
    {
        var progress = new ExerciseProgress
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            ExerciseId = request.ExerciseId,
            SessionId = request.SessionId,
            SessionDate = request.SessionDate?.Date,
            BestSetWeight = request.BestSetWeight,
            BestSetReps = request.BestSetReps,
            TotalVolume = request.TotalVolume,
            OneRmEstimate = request.OneRmEstimate,
        };
        _db.ExerciseProgress.Add(progress);
        await _db.SaveChangesAsync(cancellationToken);
        return Created($"api/me/exercise-progress/{progress.Id}", progress);
    }
}
