using CoreGym.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CoreGym.Infrastructure.Services;

public static class ApplicationServiceSetup
{
    /// <summary>
    /// Registers the Phase 2 application services that replace the original
    /// SECURITY DEFINER functions, triggers and RPCs (streaks, daily-summary
    /// sync, messaging, notifications, subscription lifecycle, coach rating,
    /// profile provisioning).
    /// </summary>
    public static IServiceCollection AddCoreGymApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IStreakService, StreakService>();
        services.AddScoped<IDailySummaryService, DailySummaryService>();
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ISubscriptionLifecycleService, SubscriptionLifecycleService>();
        services.AddScoped<ICoachRatingService, CoachRatingService>();
        services.AddScoped<IProfileProvisioningService, ProfileProvisioningService>();

        return services;
    }
}
