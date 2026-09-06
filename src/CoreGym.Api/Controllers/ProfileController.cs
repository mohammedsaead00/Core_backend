using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Services;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public class ProfileController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;
    private readonly IProfileProvisioningService _provisioning;

    public ProfileController(CoreGymDbContext db, ICurrentUserService currentUser, IProfileProvisioningService provisioning)
        : base(currentUser)
    {
        _db = db;
        _provisioning = provisioning;
    }

    /// <summary>Provisions the profile for the authenticated user (handle_new_user replacement; idempotent).</summary>
    [HttpPost]
    public async Task<IActionResult> Provision([FromBody] ProvisionProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await _provisioning.ProvisionAsync(UserId, request.Email, request.Name, cancellationToken);
        return Created("api/me", profile);
    }

    [HttpGet]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var profile = await _db.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == UserId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.Id == UserId, cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }

        if (request.Name is not null) profile.Name = request.Name;
        if (request.FullName is not null) profile.FullName = request.FullName;
        if (request.Gender is not null) profile.Gender = request.Gender;
        if (request.Age is not null) profile.Age = request.Age;
        if (request.WeightKg is not null) profile.WeightKg = request.WeightKg;
        if (request.HeightCm is not null) profile.HeightCm = request.HeightCm;
        if (request.FitnessGoal is not null) profile.FitnessGoal = request.FitnessGoal;
        if (request.AvatarUrl is not null) profile.AvatarUrl = request.AvatarUrl;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(profile);
    }
}
