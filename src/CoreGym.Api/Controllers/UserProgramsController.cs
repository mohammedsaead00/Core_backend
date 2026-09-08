using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

/// <summary>Full own CRUD for custom user programs (matches the original RLS).</summary>
[ApiController]
[Route("api/me/user-programs")]
[Authorize]
public class UserProgramsController : CoreGymControllerBase
{
    // CHECK-constrained values in the live prod schema.
    private static readonly string[] AllowedMuscleGroups = ["chest", "arms", "legs", "core"];

    private readonly CoreGymDbContext _db;

    public UserProgramsController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var programs = await _db.UserPrograms
            .AsNoTracking()
            .Where(p => p.UserId == UserId)
            .OrderByDescending(p => p.IsActive)
            .ThenByDescending(p => p.StartedAt)
            .ToListAsync(cancellationToken);
        return Ok(programs);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUpdateUserProgramRequest request, CancellationToken cancellationToken)
    {
        ValidateMuscleGroup(request.MuscleGroup);
        var program = new UserProgram
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            ProgramName = request.ProgramName,
            MuscleGroup = request.MuscleGroup,
            IsActive = request.IsActive,
            StartedAt = request.StartedAt ?? DateTimeOffset.UtcNow,
        };
        _db.UserPrograms.Add(program);
        await _db.SaveChangesAsync(cancellationToken);
        return Created($"api/me/user-programs/{program.Id}", program);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateUpdateUserProgramRequest request, CancellationToken cancellationToken)
    {
        ValidateMuscleGroup(request.MuscleGroup);
        var program = await _db.UserPrograms
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == UserId, cancellationToken);
        if (program is null)
        {
            return NotFound();
        }

        program.ProgramName = request.ProgramName;
        program.MuscleGroup = request.MuscleGroup;
        if (request.IsActive is not null) program.IsActive = request.IsActive;
        if (request.StartedAt is not null) program.StartedAt = request.StartedAt;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(program);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var program = await _db.UserPrograms
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == UserId, cancellationToken);
        if (program is null)
        {
            return NotFound();
        }

        _db.UserPrograms.Remove(program);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static void ValidateMuscleGroup(string? muscleGroup)
    {
        var normalized = muscleGroup?.Trim().ToLowerInvariant();
        if (normalized is null || !AllowedMuscleGroups.Contains(normalized))
        {
            throw new ArgumentException("Muscle group must be one of: chest, arms, legs, core.");
        }
    }
}
