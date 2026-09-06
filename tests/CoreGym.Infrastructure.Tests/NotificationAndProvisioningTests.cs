using CoreGym.Domain.Entities;
using CoreGym.Domain.Enums;
using CoreGym.Infrastructure;
using CoreGym.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Verifies the mark_notification_read / mark_all_notifications_read
/// replacements (own-only writes) and the handle_new_user replacement
/// (idempotent profile provisioning).
/// </summary>
[Collection("sql-smoke")]
public class NotificationAndProvisioningTests
{
    private readonly SqlServerSmokeFixture _fx;

    public NotificationAndProvisioningTests(SqlServerSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Mark_notification_read_only_affects_own_notifications()
    {
        var userA = await CreateUserAsync(_fx.Context);
        var userB = await CreateUserAsync(_fx.Context);
        var own = await CreateNotificationAsync(_fx.Context, userA);
        var foreign = await CreateNotificationAsync(_fx.Context, userB);

        await using var ctx = _fx.CreateContext();
        var service = new NotificationService(ctx);

        Assert.Equal(0, await service.MarkNotificationReadAsync(foreign, userA));
        Assert.Equal(1, await service.MarkNotificationReadAsync(own, userA));
        Assert.Equal(0, await service.MarkNotificationReadAsync(own, userA)); // already read

        await using var ctx2 = _fx.CreateContext();
        Assert.True((await ctx2.Notifications.AsNoTracking().SingleAsync(n => n.Id == own)).IsRead);
        Assert.False((await ctx2.Notifications.AsNoTracking().SingleAsync(n => n.Id == foreign)).IsRead);
    }

    [Fact]
    public async Task Mark_all_notifications_read_only_touches_the_caller()
    {
        var userA = await CreateUserAsync(_fx.Context);
        var userB = await CreateUserAsync(_fx.Context);
        await CreateNotificationAsync(_fx.Context, userA);
        await CreateNotificationAsync(_fx.Context, userA);
        await CreateNotificationAsync(_fx.Context, userA, alreadyRead: true);
        await CreateNotificationAsync(_fx.Context, userB);

        await using var ctx = _fx.CreateContext();
        Assert.Equal(2, await new NotificationService(ctx).MarkAllNotificationsReadAsync(userA));

        await using var ctx2 = _fx.CreateContext();
        Assert.True(await ctx2.Notifications.Where(n => n.UserId == userA).AllAsync(n => n.IsRead));
        Assert.False(await ctx2.Notifications.Where(n => n.UserId == userB).AllAsync(n => n.IsRead));
    }

    [Fact]
    public async Task Profile_provisioning_is_idempotent()
    {
        var userId = Guid.NewGuid();

        await using (var ctx = _fx.CreateContext())
        {
            var profile = await new ProfileProvisioningService(ctx).ProvisionAsync(userId, email: "user@example.com", name: "New User");
            Assert.Equal(UserRole.Client, profile.Role);
            Assert.Equal("user@example.com", profile.Email);
        }

        await using (var ctx2 = _fx.CreateContext())
        {
            var again = await new ProfileProvisioningService(ctx2).ProvisionAsync(userId, email: "other@example.com");
            Assert.Equal("user@example.com", again.Email); // existing row untouched
        }

        await using var ctx3 = _fx.CreateContext();
        Assert.Equal(1, await ctx3.Profiles.CountAsync(p => p.Id == userId));
    }

    private static async Task<Guid> CreateUserAsync(CoreGymDbContext ctx, string name = "notif-user")
    {
        var profile = new Profile { Id = Guid.NewGuid(), Name = name };
        ctx.Profiles.Add(profile);
        await ctx.SaveChangesAsync();
        return profile.Id;
    }

    private static async Task<Guid> CreateNotificationAsync(CoreGymDbContext ctx, Guid userId, bool alreadyRead = false)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = "test",
            Title = "t",
            Body = "b",
            IsRead = alreadyRead,
        };
        ctx.Notifications.Add(notification);
        await ctx.SaveChangesAsync();
        return notification.Id;
    }
}
