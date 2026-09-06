using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/me/onboarding")]
[Authorize]
public class OnboardingController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public OnboardingController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var onboarding = await _db.Onboardings
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.UserId == UserId, cancellationToken);
        return Ok(onboarding);
    }

    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpsertOnboardingRequest request, CancellationToken cancellationToken)
    {
        var onboarding = await _db.Onboardings.FirstOrDefaultAsync(o => o.UserId == UserId, cancellationToken);
        if (onboarding is null)
        {
            onboarding = new Onboarding { Id = Guid.NewGuid(), UserId = UserId };
            _db.Onboardings.Add(onboarding);
        }

        if (request.Age is not null) onboarding.Age = request.Age;
        if (request.Gender is not null) onboarding.Gender = request.Gender;
        if (request.HeightCm is not null) onboarding.HeightCm = request.HeightCm;
        if (request.WeightKg is not null) onboarding.WeightKg = request.WeightKg;
        if (request.Goal is not null) onboarding.Goal = request.Goal;
        if (request.ActivityLevel is not null) onboarding.ActivityLevel = request.ActivityLevel;
        if (request.TargetWeight is not null) onboarding.TargetWeight = request.TargetWeight;
        if (request.WeeklyWorkouts is not null) onboarding.WeeklyWorkouts = request.WeeklyWorkouts;
        onboarding.Completed = request.Completed;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(onboarding);
    }
}
