using CoreGym.Domain.Services;
using CoreGym.Infrastructure.AI;
using CoreGym.Infrastructure.Notifications;
using CoreGym.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreGym.Infrastructure.Integrations;

public static class IntegrationServiceSetup
{
    /// <summary>
    /// Registers the external integrations: OneSignal push (no-op when
    /// unconfigured), Stripe webhook handling, Gemini-backed AI services,
    /// file storage, and the meal reminder service.
    /// </summary>
    public static IServiceCollection AddCoreGymIntegrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OneSignalOptions>(configuration.GetSection(OneSignalOptions.SectionName));
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));

        services.AddHttpClient<IPushNotificationService, OneSignalPushService>();
        services.AddHttpClient<IGeminiClient, GeminiClient>();
        services.AddHttpClient<IBarcodeLookupService, BarcodeLookupService>();
        services.AddScoped<IFoodAnalysisService, FoodAnalysisService>();
        services.AddScoped<IMealReminderService, MealReminderService>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddScoped<IStripeWebhookService, StripeWebhookService>();

        return services;
    }
}
