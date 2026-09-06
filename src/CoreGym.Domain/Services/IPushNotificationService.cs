namespace CoreGym.Domain.Services;

/// <summary>
/// Push delivery via OneSignal, targeting users by external_id alias
/// (= the app user id, as in the original send-chat-push flow).
/// Implementations must be safe when push is not configured (no-op).
/// </summary>
public interface IPushNotificationService
{
    Task SendToUsersAsync(
        IReadOnlyCollection<Guid> userIds,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);
}
