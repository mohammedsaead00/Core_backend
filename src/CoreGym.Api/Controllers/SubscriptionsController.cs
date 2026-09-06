using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
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

    public SubscriptionsController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
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
}
