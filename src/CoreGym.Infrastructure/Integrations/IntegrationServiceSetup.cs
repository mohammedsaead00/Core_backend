using CoreGym.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreGym.Infrastructure.Integrations;

public static class IntegrationServiceSetup
{
    /// <summary>
    /// Registers the external integrations: OneSignal push (no-op when
    /// unconfigured), Stripe webhook handling and their options.
    /// </summary>
    public static IServiceCollection AddCoreGymIntegrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OneSignalOptions>(configuration.GetSection(OneSignalOptions.SectionName));
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));

        services.AddHttpClient<IPushNotificationService, OneSignalPushService>();
        services.AddScoped<IStripeWebhookService, StripeWebhookService>();

        return services;
    }
}
