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
[Route("api/nutrition/logs")]
[Authorize]
public class NutritionController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;
    private readonly IDailySummaryService _summary;

    public NutritionController(CoreGymDbContext db, IDailySummaryService summary, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
        _summary = summary;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] DateOnly? date, [FromQuery] int limit = 100, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 500);
        var query = _db.NutritionLogs.AsNoTracking().Where(n => n.UserId == UserId);
        if (date is not null)
        {
            query = query.Where(n => n.LoggedDate == date.Value.ToDateTime(TimeOnly.MinValue));
        }

        var logs = await query.OrderByDescending(n => n.LoggedAt).Take(limit).ToListAsync(cancellationToken);
        return Ok(logs);
    }

    /// <summary>Creates a log entry and re-syncs the daily summary, replacing the sync trigger.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNutritionLogRequest request, CancellationToken cancellationToken)
    {
        var log = new NutritionLog
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            FoodId = request.FoodId,
            FoodName = request.FoodName,
            MealType = request.MealType,
            Quantity = request.Quantity,
            ServingUnit = request.ServingUnit,
            Calories = request.Calories,
            ProteinG = request.ProteinG,
            CarbsG = request.CarbsG,
            FatG = request.FatG,
            LoggedDate = request.Date?.Date,
            LoggedAt = DateTimeOffset.UtcNow,
        };
        _db.NutritionLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);

        await _summary.SyncNutritionAsync(UserId, log.LoggedDate ?? DateTime.UtcNow.Date, cancellationToken);
        return Created($"api/nutrition/logs/{log.Id}", log);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var log = await _db.NutritionLogs
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == UserId, cancellationToken);
        if (log is null)
        {
            return NotFound();
        }

        _db.NutritionLogs.Remove(log);
        await _db.SaveChangesAsync(cancellationToken);

        await _summary.SyncNutritionAsync(UserId, log.LoggedDate ?? DateTime.UtcNow.Date, cancellationToken);
        return NoContent();
    }
}
