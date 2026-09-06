using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/exercises")]
public class ExercisesController : ControllerBase
{
    private readonly CoreGymDbContext _db;

    public ExercisesController(CoreGymDbContext db)
    {
        _db = db;
    }

    /// <summary>Public reference data (public read in the original RLS).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? muscleGroup,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Exercises.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(e =>
                EF.Functions.Like(e.Name, pattern)
                || (e.NameAr != null && EF.Functions.Like(e.NameAr, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(muscleGroup))
        {
            query = query.Where(e => e.MuscleGroup == muscleGroup);
        }

        var items = await query
            .OrderBy(e => e.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var exercise = await _db.Exercises.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        return exercise is null ? NotFound() : Ok(exercise);
    }
}
