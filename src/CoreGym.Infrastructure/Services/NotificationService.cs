using CoreGym.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly CoreGymDbContext _db;

    public NotificationService(CoreGymDbContext db)
    {
        _db = db;
    }

    public async Task<int> MarkNotificationReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
        => await _db.Notifications
            .Where(n => n.Id == notificationId && n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(u => u.SetProperty(n => n.IsRead, true), cancellationToken);

    public async Task<int> MarkAllNotificationsReadAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(u => u.SetProperty(n => n.IsRead, true), cancellationToken);
}
