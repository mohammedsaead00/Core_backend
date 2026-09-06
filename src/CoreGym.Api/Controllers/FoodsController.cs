using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/foods")]
public class FoodsController : ControllerBase
{
    private readonly CoreGymDbContext _db;

    public FoodsController(CoreGymDbContext db)
    {
        _db = db;
    }

    /// <summary>Public reference data (public read in the original RLS).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Foods.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(f =>
                EF.Functions.Like(f.Name, pattern)
                || (f.NameAr != null && EF.Functions.Like(f.NameAr, pattern)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(f => f.Category == category);
        }

        var items = await query
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var food = await _db.Foods.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        return food is null ? NotFound() : Ok(food);
    }
}
