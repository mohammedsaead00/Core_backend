namespace CoreGym.Domain.Entities;

/// <summary>Per-user push/notification preferences. Never deleted, only updated.</summary>
public class NotificationPreference
{
    public Guid UserId { get; set; }

    public bool? MealRemindersEnabled { get; set; }

    public bool? WaterRemindersEnabled { get; set; }

    public bool? CalorieAlertsEnabled { get; set; }

    public bool? ChatNotificationsEnabled { get; set; }

    public TimeSpan? QuietHoursStart { get; set; }

    public TimeSpan? QuietHoursEnd { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Profile? User { get; set; }
}
