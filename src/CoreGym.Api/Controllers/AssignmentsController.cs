using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Controllers;

/// <summary>Coaches assigning content to clients; both sides can list their assignments.</summary>
[ApiController]
[Authorize]
public class AssignmentsController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;

    public AssignmentsController(CoreGymDbContext db, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
    }

    [HttpPost("api/coach/assignments")]
    public async Task<IActionResult> Create([FromBody] CreateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var coach = await _db.Coaches.SingleOrDefaultAsync(c => c.UserId == UserId, cancellationToken)
            ?? throw new KeyNotFoundException("No coach profile exists for the current user.");

        var contentExists = await _db.CoachContents.AnyAsync(cc => cc.Id == request.ContentId && cc.CoachId == coach.Id, cancellationToken);
        if (!contentExists)
        {
            return NotFound(new { title = "Content not found for this coach." });
        }

        var clientExists = await _db.Profiles.AnyAsync(p => p.Id == request.ClientId, cancellationToken);
        if (!clientExists)
        {
            return NotFound(new { title = "Client not found." });
        }

        var assignment = new ClientAssignment
        {
            Id = Guid.NewGuid(),
            CoachId = coach.Id,
            ClientId = request.ClientId,
            ContentId = request.ContentId,
            Note = request.Note,
            AssignedAt = DateTimeOffset.UtcNow,
        };
        _db.ClientAssignments.Add(assignment);
        await _db.SaveChangesAsync(cancellationToken);
        return Created($"api/coach/assignments/{assignment.Id}", assignment);
    }

    [HttpGet("api/coach/assignments")]
    public async Task<IActionResult> CoachList(CancellationToken cancellationToken)
    {
        var assignments = await Project(
            _db.ClientAssignments.AsNoTracking().Where(a => a.CoachId == UserId).OrderByDescending(a => a.AssignedAt))
            .ToListAsync(cancellationToken);
        return Ok(assignments);
    }

    [HttpGet("api/me/assignments")]
    public async Task<IActionResult> ClientList(CancellationToken cancellationToken)
    {
        var assignments = await Project(
            _db.ClientAssignments.AsNoTracking().Where(a => a.ClientId == UserId).OrderByDescending(a => a.AssignedAt))
            .ToListAsync(cancellationToken);
        return Ok(assignments);
    }

    // Filters/ordering must run on the entity query; the projection (with its
    // correlated subqueries) has to be the outermost operation for EF to translate.
    private IQueryable<AssignmentResponse> Project(IQueryable<ClientAssignment> query) =>
        query.Select(a => new AssignmentResponse(
            a.Id, a.CoachId, a.ClientId, a.ContentId, a.Note, a.AssignedAt,
            _db.CoachContents.Where(c => c.Id == a.ContentId).Select(c => c.Title).FirstOrDefault(),
            _db.CoachContents.Where(c => c.Id == a.ContentId).Select(c => c.Type).FirstOrDefault(),
            _db.CoachContents.Where(c => c.Id == a.ContentId).Select(c => c.FileUrl).FirstOrDefault()));
}
