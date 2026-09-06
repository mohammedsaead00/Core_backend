using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/me/goals")]
[Authorize]
public class GoalsController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public GoalsController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var goals = await _db.UserGoals
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.UserId == UserId, cancellationToken);
        return Ok(goals);
    }

    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpsertGoalsRequest request, CancellationToken cancellationToken)
    {
        var goals = await _db.UserGoals.FirstOrDefaultAsync(g => g.UserId == UserId, cancellationToken);
        if (goals is null)
        {
            goals = new UserGoal { Id = Guid.NewGuid(), UserId = UserId };
            _db.UserGoals.Add(goals);
        }

        if (request.DailyCalories is not null) goals.DailyCalories = request.DailyCalories;
        if (request.DailyProteinG is not null) goals.DailyProteinG = request.DailyProteinG;
        if (request.DailyCarbsG is not null) goals.DailyCarbsG = request.DailyCarbsG;
        if (request.DailyFatG is not null) goals.DailyFatG = request.DailyFatG;
        if (request.DailyWaterMl is not null) goals.DailyWaterMl = request.DailyWaterMl;
        if (request.DailySteps is not null) goals.DailySteps = request.DailySteps;
        if (request.DailySleepHours is not null) goals.DailySleepHours = request.DailySleepHours;
        if (request.WeeklyWorkouts is not null) goals.WeeklyWorkouts = request.WeeklyWorkouts;
        if (request.TargetWeightKg is not null) goals.TargetWeightKg = request.TargetWeightKg;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(goals);
    }
}
