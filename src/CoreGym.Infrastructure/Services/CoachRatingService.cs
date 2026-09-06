using CoreGym.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Services;

public class CoachRatingService : ICoachRatingService
{
    private readonly CoreGymDbContext _db;

    public CoachRatingService(CoreGymDbContext db)
    {
        _db = db;
    }

    public async Task RecalculateCoachRatingAsync(Guid coachId, CancellationToken cancellationToken = default)
    {
        var reviews = _db.Reviews.Where(r => r.CoachId == coachId);
        var count = await reviews.CountAsync(cancellationToken);
        var rating = count == 0
            ? 0m
            : Math.Round(await reviews.SumAsync(r => (decimal)r.Rating, cancellationToken) / count, 2);

        var coach = await _db.Coaches.SingleOrDefaultAsync(c => c.Id == coachId, cancellationToken)
            ?? throw new KeyNotFoundException($"Coach '{coachId}' was not found.");
        coach.Rating = rating;

        var profile = await _db.CoachProfiles.SingleOrDefaultAsync(cp => cp.Id == coach.UserId, cancellationToken);
        if (profile is not null)
        {
            profile.Rating = rating;
            profile.ReviewsCount = count;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
