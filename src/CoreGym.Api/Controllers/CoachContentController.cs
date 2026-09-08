using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

/// <summary>Coach-managed content library plus the client-readable view (public or assigned items).</summary>
[ApiController]
[Authorize]
public class CoachContentController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public CoachContentController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    private async Task<Coach> ResolveCoachAsync(CancellationToken cancellationToken) =>
        await _db.Coaches.SingleOrDefaultAsync(c => c.UserId == UserId, cancellationToken)
            ?? throw new KeyNotFoundException("No coach profile exists for the current user.");

    [HttpGet("api/coach/content")]
    public async Task<IActionResult> MyContent(CancellationToken cancellationToken)
    {
        var coach = await ResolveCoachAsync(cancellationToken);
        var items = await _db.CoachContents
            .AsNoTracking()
            .Where(cc => cc.CoachId == coach.Id)
            .OrderBy(cc => cc.SortOrder)
            .ThenByDescending(cc => cc.CreatedAt)
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost("api/coach/content")]
    public async Task<IActionResult> Create([FromBody] CreateCoachContentRequest request, CancellationToken cancellationToken)
    {
        var coach = await ResolveCoachAsync(cancellationToken);
        var content = new CoachContent
        {
            Id = Guid.NewGuid(),
            CoachId = coach.Id,
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            FileUrl = request.FileUrl,
            IsPublic = request.IsPublic,
            ThumbnailUrl = request.ThumbnailUrl,
            FileSizeKb = request.FileSizeKb,
            SortOrder = request.SortOrder,
        };
        _db.CoachContents.Add(content);
        await _db.SaveChangesAsync(cancellationToken);
        return Created($"api/coach/content/{content.Id}", content);
    }

    [HttpPut("api/coach/content/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCoachContentRequest request, CancellationToken cancellationToken)
    {
        var coach = await ResolveCoachAsync(cancellationToken);
        var content = await _db.CoachContents
            .FirstOrDefaultAsync(cc => cc.Id == id && cc.CoachId == coach.Id, cancellationToken);
        if (content is null)
        {
            return NotFound();
        }

        if (request.Title is not null) content.Title = request.Title;
        if (request.Type is not null) content.Type = request.Type;
        if (request.FileUrl is not null) content.FileUrl = request.FileUrl;
        if (request.Description is not null) content.Description = request.Description;
        if (request.IsPublic is not null) content.IsPublic = request.IsPublic.Value;
        if (request.ThumbnailUrl is not null) content.ThumbnailUrl = request.ThumbnailUrl;
        if (request.FileSizeKb is not null) content.FileSizeKb = request.FileSizeKb;
        if (request.SortOrder is not null) content.SortOrder = request.SortOrder.Value;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(content);
    }

    [HttpDelete("api/coach/content/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var coach = await ResolveCoachAsync(cancellationToken);
        var content = await _db.CoachContents
            .FirstOrDefaultAsync(cc => cc.Id == id && cc.CoachId == coach.Id, cancellationToken);
        if (content is null)
        {
            return NotFound();
        }

        _db.CoachContents.Remove(content);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Content that is assigned to clients is protected by the Restrict FK.
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Content in use",
                detail: "This content is assigned to at least one client and cannot be deleted.");
        }

        return NoContent();
    }

    /// <summary>Client-facing list: public items plus anything assigned to the caller (per the original RLS).</summary>
    [HttpGet("api/coaches/{coachId:guid}/content")]
    public async Task<IActionResult> ClientView(Guid coachId, CancellationToken cancellationToken)
    {
        var items = await _db.CoachContents
            .AsNoTracking()
            .Where(cc => cc.CoachId == coachId
                         && (cc.IsPublic
                             || _db.ClientAssignments.Any(a => a.ContentId == cc.Id && a.ClientId == UserId)))
            .OrderBy(cc => cc.SortOrder)
            .ToListAsync(cancellationToken);
        return Ok(items);
    }
}
