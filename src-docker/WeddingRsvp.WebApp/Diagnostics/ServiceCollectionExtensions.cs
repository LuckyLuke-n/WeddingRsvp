using System.Reflection;
using OpenTelemetry.Resources;
using WeddingRsvp.WebApp.Diagnostics.Meters;

namespace WeddingRsvp.WebApp.Diagnostics;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddWebAppOpenTelemetry( this IServiceCollection services )
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource
                    .AddService("RsvpApi", "WeddingRsvp.WebApp",
                        Assembly.GetExecutingAssembly().GetName().Version!.ToString())
                    .AddAttributes(
                    [

                        new KeyValuePair<string, object>("service.hostname", Environment.MachineName)
                    ]);
            })
            .WithMetrics( meters =>
                meters.AddMeter( WebAppMeter.Name )
            )
            .WithLogging()
            .WithTracing();

        return services;
    }
}