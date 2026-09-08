using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

/// <summary>
/// Subscription phases: the coach manages phases of their own subscriptions;
/// the client reads the phases of their own subscription (per the original RLS).
/// </summary>
[ApiController]
[Authorize]
public class SubscriptionPhasesController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public SubscriptionPhasesController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    private async Task<Subscription?> ResolveCoachSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken) =>
        await _db.Subscriptions
            .Include(s => s.Coach)
            .SingleOrDefaultAsync(s => s.Id == subscriptionId && s.Coach!.UserId == UserId, cancellationToken);

    [HttpGet("api/coach/subscriptions/{subscriptionId:guid}/phases")]
    public async Task<IActionResult> CoachList(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await ResolveCoachSubscriptionAsync(subscriptionId, cancellationToken);
        if (subscription is null)
        {
            return NotFound();
        }

        return Ok(await ListPhasesAsync(subscriptionId, cancellationToken));
    }

    [HttpPost("api/coach/subscriptions/{subscriptionId:guid}/phases")]
    public async Task<IActionResult> Create(Guid subscriptionId, [FromBody] CreatePhaseRequest request, CancellationToken cancellationToken)
    {
        var subscription = await ResolveCoachSubscriptionAsync(subscriptionId, cancellationToken);
        if (subscription is null)
        {
            return NotFound();
        }

        var phase = new SubscriptionPhase
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscriptionId,
            PhaseNumber = request.PhaseNumber,
            Title = request.Title,
            Type = request.Type,
            Description = request.Description,
            DurationWeeks = request.DurationWeeks,
            Status = request.Status,
        };
        _db.SubscriptionPhases.Add(phase);
        await _db.SaveChangesAsync(cancellationToken);
        return Created($"api/coach/subscriptions/{subscriptionId}/phases/{phase.Id}", phase);
    }

    [HttpPut("api/coach/subscriptions/{subscriptionId:guid}/phases/{phaseId:guid}")]
    public async Task<IActionResult> Update(Guid subscriptionId, Guid phaseId, [FromBody] UpdatePhaseRequest request, CancellationToken cancellationToken)
    {
        var subscription = await ResolveCoachSubscriptionAsync(subscriptionId, cancellationToken);
        if (subscription is null)
        {
            return NotFound();
        }

        var phase = await _db.SubscriptionPhases
            .FirstOrDefaultAsync(p => p.Id == phaseId && p.SubscriptionId == subscriptionId, cancellationToken);
        if (phase is null)
        {
            return NotFound();
        }

        if (request.Title is not null) phase.Title = request.Title;
        if (request.Type is not null) phase.Type = request.Type;
        if (request.Description is not null) phase.Description = request.Description;
        if (request.DurationWeeks is not null) phase.DurationWeeks = request.DurationWeeks;
        if (request.Status is not null) phase.Status = request.Status;
        if (request.StartedAt is not null) phase.StartedAt = request.StartedAt;
        if (request.CompletedAt is not null) phase.CompletedAt = request.CompletedAt;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(phase);
    }

    [HttpDelete("api/coach/subscriptions/{subscriptionId:guid}/phases/{phaseId:guid}")]
    public async Task<IActionResult> Delete(Guid subscriptionId, Guid phaseId, CancellationToken cancellationToken)
    {
        var subscription = await ResolveCoachSubscriptionAsync(subscriptionId, cancellationToken);
        if (subscription is null)
        {
            return NotFound();
        }

        var phase = await _db.SubscriptionPhases
            .FirstOrDefaultAsync(p => p.Id == phaseId && p.SubscriptionId == subscriptionId, cancellationToken);
        if (phase is null)
        {
            return NotFound();
        }

        _db.SubscriptionPhases.Remove(phase);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("api/me/subscriptions/{subscriptionId:guid}/phases")]
    public async Task<IActionResult> ClientList(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var owned = await _db.Subscriptions
            .AnyAsync(s => s.Id == subscriptionId && s.ClientId == UserId, cancellationToken);
        if (!owned)
        {
            return NotFound();
        }

        return Ok(await ListPhasesAsync(subscriptionId, cancellationToken));
    }

    private async Task<List<PhaseResponse>> ListPhasesAsync(Guid subscriptionId, CancellationToken cancellationToken) =>
        await _db.SubscriptionPhases
            .AsNoTracking()
            .Where(p => p.SubscriptionId == subscriptionId)
            .OrderBy(p => p.PhaseNumber)
            .Select(p => new PhaseResponse(
                p.Id, p.SubscriptionId, p.PhaseNumber, p.Title, p.Type, p.Description,
                p.DurationWeeks, p.Status, p.StartedAt, p.CompletedAt))
            .ToListAsync(cancellationToken);
}
