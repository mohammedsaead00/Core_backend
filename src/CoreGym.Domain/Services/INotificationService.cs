namespace CoreGym.Domain.Services;

/// <summary>
/// Replaces the mark_notification_read / mark_all_notifications_read RPCs
/// (server-side writes to the notifications table, mirroring the original
/// SECURITY DEFINER functions — clients never write directly).
/// </summary>
public interface INotificationService
{
    /// <summary>Marks one of the user's own notifications read; returns 1 when it existed, otherwise 0.</summary>
    Task<int> MarkNotificationReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Marks all of the user's notifications read; returns the number updated.</summary>
    Task<int> MarkAllNotificationsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
