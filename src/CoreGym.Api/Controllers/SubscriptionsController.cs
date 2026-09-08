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
[Route("api")]
[Authorize]
public class SubscriptionsController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;
    private readonly ISubscriptionLifecycleService _lifecycle;

    public SubscriptionsController(CoreGymDbContext db, ISubscriptionLifecycleService lifecycle, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
        _lifecycle = lifecycle;
    }

    /// <summary>The calling user's own subscriptions as a client (subscriptions_client_read).</summary>
    [HttpGet("me/subscriptions")]
    public async Task<IActionResult> MySubscriptions(CancellationToken cancellationToken)
    {
        var subscriptions = await _db.Subscriptions
            .AsNoTracking()
            .Where(s => s.ClientId == UserId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
        return Ok(subscriptions);
    }

    /// <summary>Subscriptions of the calling coach (subscriptions_coach_read path).</summary>
    [HttpGet("coach/subscriptions")]
    public async Task<IActionResult> CoachSubscriptions(CancellationToken cancellationToken)
    {
        var subscriptions = await _db.Subscriptions
            .AsNoTracking()
            .Where(s => s.Coach!.UserId == UserId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new CoachSubscriptionResponse(
                s.Id, s.ClientId, s.Status, s.Tier, s.StartDate, s.EndDate, s.PaymentStatus))
            .ToListAsync(cancellationToken);
        return Ok(subscriptions);
    }

    /// <summary>The coach dashboard client list: active subscriptions joined to client profiles.</summary>
    [HttpGet("coach/clients")]
    public async Task<IActionResult> CoachClients(CancellationToken cancellationToken)
    {
        var clients = await _db.Subscriptions
            .AsNoTracking()
            .Where(s => s.Coach!.UserId == UserId && s.Status == "active")
            .Join(_db.Profiles.AsNoTracking(),
                s => s.ClientId,
                p => p.Id,
                (s, p) => new { s, p })
            .OrderBy(x => x.p.Name ?? x.p.FullName)
            .Select(x => new CoachClientResponse(
                x.s.Id, x.p.Id, x.p.Name ?? x.p.FullName, x.p.AvatarUrl,
                x.s.Status, x.s.Tier, x.s.StartDate, x.s.EndDate, x.s.ExpiresAt))
            .ToListAsync(cancellationToken);
        return Ok(clients);
    }

    /// <summary>
    /// Coach creates a subscription for a client without going through Stripe
    /// (the prod subscription_repository writes rows directly). Created as
    /// 'pending'; activate via PATCH status so the lifecycle side effects run.
    /// </summary>
    [HttpPost("coach/subscriptions")]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var normalizedTier = (request.Tier ?? "basic").Trim().ToLowerInvariant();
        if (normalizedTier is not ("basic" or "standard" or "premium"))
        {
            throw new ArgumentException("Tier must be basic, standard or premium.");
        }

        var coach = await _db.Coaches.SingleOrDefaultAsync(c => c.UserId == UserId, cancellationToken)
            ?? throw new KeyNotFoundException("No coach profile exists for the current user.");

        var clientExists = await _db.Profiles.AnyAsync(p => p.Id == request.ClientId, cancellationToken);
        if (!clientExists)
        {
            return NotFound(new { title = "Client not found." });
        }

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            CoachId = coach.Id,
            Status = "pending",
            Tier = normalizedTier,
            PlanId = request.PlanId,
            EndDate = request.EndDate?.Date,
            ExpiresAt = request.ExpiresAt,
            Goals = request.Goals,
            Notes = request.Notes,
        };
        _db.Subscriptions.Add(subscription);
        await _db.SaveChangesAsync(cancellationToken);
        return Created($"api/coach/subscriptions/{subscription.Id}", subscription);
    }

    /// <summary>
    /// Coach transitions a subscription's status (e.g. pending → active for
    /// non-Stripe flows). Delegates to the lifecycle service so conversations,
    /// client counting and the authorization cache stay consistent.
    /// </summary>
    [HttpPatch("coach/subscriptions/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateSubscriptionStatusRequest request, CancellationToken cancellationToken)
    {
        var owned = await _db.Subscriptions
            .AnyAsync(s => s.Id == id && s.Coach!.UserId == UserId, cancellationToken);
        if (!owned)
        {
            return NotFound();
        }

        await _lifecycle.ProcessSubscriptionStatusAsync(id, request.Status, cancellationToken);
        var status = await _db.Subscriptions
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => s.Status)
            .SingleAsync(cancellationToken);
        return Ok(new { id, status });
    }
}
