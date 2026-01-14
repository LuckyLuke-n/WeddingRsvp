using System.Reflection;
using OpenTelemetry.Resources;
using WeddingRsvp.Api.Diagnostics.Meters;

namespace WeddingRsvp.Api.Diagnostics;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddAmbientCollectorOpenTelemetry( this IServiceCollection services )
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource
                    .AddService("RsvpApi", "WeddingRsvp.Api",
                        Assembly.GetExecutingAssembly().GetName().Version!.ToString())
                    .AddAttributes(
                    [

                        new KeyValuePair<string, object>("service.hostname", Environment.MachineName)
                    ]);
            })
            .WithMetrics( meters =>
                meters.AddMeter( ResponseMeter.Name )
            )
            .WithLogging()
            .WithTracing();

        return services;
    }
}