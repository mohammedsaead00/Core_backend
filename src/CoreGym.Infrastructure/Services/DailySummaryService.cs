using CoreGym.Domain.Entities;
using CoreGym.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Services;

public class DailySummaryService : IDailySummaryService
{
    private readonly CoreGymDbContext _db;

    public DailySummaryService(CoreGymDbContext db)
    {
        _db = db;
    }

    public async Task SyncNutritionAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default)
    {
        date = date.Date;
        var totals = await _db.NutritionLogs
            .Where(n => n.UserId == userId && n.LoggedDate == date)
            .Select(n => new { n.Calories, n.ProteinG, n.CarbsG, n.FatG })
            .ToListAsync(cancellationToken);

        var summary = await GetOrCreateAsync(userId, date, createWhenMissing: totals.Count > 0, cancellationToken);
        if (summary is null)
        {
            return;
        }

        summary.CaloriesConsumed = totals.Sum(t => t.Calories);
        summary.ProteinG = totals.Sum(t => t.ProteinG);
        summary.CarbsG = totals.Sum(t => t.CarbsG);
        summary.FatG = totals.Sum(t => t.FatG);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SyncWorkoutAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default)
    {
        date = date.Date;
        var sessions = await _db.WorkoutSessions
            .Where(s => s.UserId == userId && s.SessionDate == date)
            .Select(s => new { s.DurationMin })
            .ToListAsync(cancellationToken);

        var summary = await GetOrCreateAsync(userId, date, createWhenMissing: sessions.Count > 0, cancellationToken);
        if (summary is null)
        {
            return;
        }

        summary.WorkoutDone = sessions.Count > 0;
        summary.WorkoutDuration = sessions.Sum(s => s.DurationMin);

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<DailySummary?> GetOrCreateAsync(Guid userId, DateTime date, bool createWhenMissing, CancellationToken cancellationToken)
    {
        var summary = await _db.DailySummaries
            .SingleOrDefaultAsync(s => s.UserId == userId && s.SummaryDate == date, cancellationToken);
        if (summary is not null)
        {
            return summary;
        }

        if (!createWhenMissing)
        {
            return null;
        }

        summary = new DailySummary { Id = Guid.NewGuid(), UserId = userId, SummaryDate = date };
        _db.DailySummaries.Add(summary);
        return summary;
    }
}
