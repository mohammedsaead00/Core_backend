using CoreGym.Domain.Authorization;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/me/summary")]
[Authorize]
public class SummaryController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public SummaryController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    [HttpGet("{date}")]
    public async Task<IActionResult> GetByDate(DateOnly date, CancellationToken cancellationToken)
    {
        var summary = await _db.DailySummaries
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.UserId == UserId && s.SummaryDate == date.ToDateTime(TimeOnly.MinValue), cancellationToken);
        return Ok(summary);
    }

    [HttpGet]
    public async Task<IActionResult> GetRange([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken)
    {
        var query = _db.DailySummaries.AsNoTracking().Where(s => s.UserId == UserId);
        if (from is not null)
        {
            query = query.Where(s => s.SummaryDate >= from.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (to is not null)
        {
            query = query.Where(s => s.SummaryDate <= to.Value.ToDateTime(TimeOnly.MinValue));
        }

        var summaries = await query.OrderBy(s => s.SummaryDate).Take(100).ToListAsync(cancellationToken);
        return Ok(summaries);
    }
}
