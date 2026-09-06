using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

[ApiController]
public class ProgramsController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public ProgramsController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    /// <summary>Public program catalog (public read in the original RLS).</summary>
    [HttpGet("api/programs")]
    [AllowAnonymous]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var programs = await _db.TrainingPrograms
            .AsNoTracking()
            .Where(p => p.IsActive != false)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
        return Ok(programs);
    }

    [HttpGet("api/programs/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var program = await _db.TrainingPrograms
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (program is null)
        {
            return NotFound();
        }

        var days = await _db.ProgramDays
            .AsNoTracking()
            .Where(d => d.ProgramId == id)
            .OrderBy(d => d.DayNumber)
            .ToListAsync(cancellationToken);
        var dayIds = days.Select(d => d.Id).ToList();
        var dayExercises = await _db.ProgramDayExercises
            .AsNoTracking()
            .Where(de => dayIds.Contains(de.ProgramDayId))
            .OrderBy(de => de.OrderIndex)
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            program,
            days = days.Select(d => new
            {
                d.Id,
                d.DayNumber,
                d.Name,
                d.NameAr,
                d.MuscleGroups,
                d.Notes,
                Exercises = dayExercises.Where(e => e.ProgramDayId == d.Id).ToList(),
            }),
        });
    }

    /// <summary>The calling user's active program pointer.</summary>
    [HttpGet("api/me/active-program")]
    [Authorize]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var active = await _db.UserActivePrograms
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == UserId, cancellationToken);
        return Ok(active);
    }

    [HttpPut("api/me/active-program")]
    [Authorize]
    public async Task<IActionResult> SetActive([FromBody] SetActiveProgramRequest request, CancellationToken cancellationToken)
    {
        var active = await _db.UserActivePrograms.FirstOrDefaultAsync(a => a.UserId == UserId, cancellationToken);
        if (active is null)
        {
            active = new UserActiveProgram { Id = Guid.NewGuid(), UserId = UserId, ProgramId = request.ProgramId };
            _db.UserActivePrograms.Add(active);
        }
        else
        {
            active.ProgramId = request.ProgramId;
        }

        if (request.CurrentWeek is not null) active.CurrentWeek = request.CurrentWeek;
        if (request.CurrentDay is not null) active.CurrentDay = request.CurrentDay;
        if (request.Notes is not null) active.Notes = request.Notes;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(active);
    }
}
