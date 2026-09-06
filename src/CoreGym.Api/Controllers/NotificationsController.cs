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
[Route("api/notifications")]
[Authorize]
public class NotificationsController : CoreGymControllerBase
{
    private readonly CoreGymDbContext _db;
    private readonly INotificationService _notifications;

    public NotificationsController(CoreGymDbContext db, INotificationService notifications, ICurrentUserService currentUser) : base(currentUser)
    {
        _db = db;
        _notifications = notifications;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 200);
        var items = await _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>mark_notification_read replacement (own-only).</summary>
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var updated = await _notifications.MarkNotificationReadAsync(id, UserId, cancellationToken);
        return updated == 0 ? NotFound() : NoContent();
    }

    /// <summary>mark_all_notifications_read replacement.</summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var updated = await _notifications.MarkAllNotificationsReadAsync(UserId, cancellationToken);
        return Ok(new MarkedReadResponse(updated));
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var preferences = await _db.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == UserId, cancellationToken);
        return Ok(preferences);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpsertPreferences([FromBody] UpdateNotificationPreferencesRequest request, CancellationToken cancellationToken)
    {
        var preferences = await _db.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == UserId, cancellationToken);
        if (preferences is null)
        {
            preferences = new NotificationPreference { UserId = UserId };
            _db.NotificationPreferences.Add(preferences);
        }

        if (request.MealRemindersEnabled is not null) preferences.MealRemindersEnabled = request.MealRemindersEnabled;
        if (request.WaterRemindersEnabled is not null) preferences.WaterRemindersEnabled = request.WaterRemindersEnabled;
        if (request.CalorieAlertsEnabled is not null) preferences.CalorieAlertsEnabled = request.CalorieAlertsEnabled;
        if (request.ChatNotificationsEnabled is not null) preferences.ChatNotificationsEnabled = request.ChatNotificationsEnabled;
        if (request.QuietHoursStart is not null) preferences.QuietHoursStart = request.QuietHoursStart;
        if (request.QuietHoursEnd is not null) preferences.QuietHoursEnd = request.QuietHoursEnd;

        await _db.SaveChangesAsync(cancellationToken);
        return Ok(preferences);
    }
}
