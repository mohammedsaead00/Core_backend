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
[Route("api/coaches")]
public class CoachesController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;
    private readonly ICoachRatingService _ratings;

    public CoachesController(CoreGymDbContext db, ICoachRatingService ratings, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
        _ratings = ratings;
    }

    /// <summary>Public coach directory (coaches_read_all in the original RLS).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var coaches = await (from c in _db.Coaches.AsNoTracking()
                             join p in _db.Profiles.AsNoTracking() on c.UserId equals p.Id
                             where c.IsActive != false
                             orderby c.Rating descending
                             select new CoachListItemResponse(
                                 c.Id, p.Name ?? p.FullName, p.AvatarUrl, c.Rating, c.Specialization, c.PriceMonthly))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return Ok(coaches);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var coach = await (from c in _db.Coaches.AsNoTracking()
                           join p in _db.Profiles.AsNoTracking() on c.UserId equals p.Id
                           where c.Id == id
                           select new CoachDetailResponse(
                               c.Id, p.Name ?? p.FullName, p.AvatarUrl, c.Bio, c.Rating, c.PriceMonthly, c.Specialization, c.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
        return coach is null ? NotFound() : Ok(coach);
    }

    /// <summary>The canonical reviews table (the app's two review read paths).</summary>
    [HttpGet("{id:guid}/reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> ListReviews(Guid id, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 200);
        var reviews = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.CoachId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return Ok(reviews);
    }

    /// <summary>Post a review as the authenticated client, then refresh the coach's rating aggregates.</summary>
    [HttpPost("{id:guid}/reviews")]
    [Authorize]
    public async Task<IActionResult> CreateReview(Guid id, [FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        if (request.Rating is < 1 or > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5.");
        }

        var coachExists = await _db.Coaches.AnyAsync(c => c.Id == id, cancellationToken);
        if (!coachExists)
        {
            return NotFound();
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            ClientId = UserId,
            CoachId = id,
            Rating = request.Rating,
            Comment = request.Comment,
        };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync(cancellationToken);

        await _ratings.RecalculateCoachRatingAsync(id, cancellationToken);
        return Created($"api/coaches/{id}/reviews", review);
    }
}
