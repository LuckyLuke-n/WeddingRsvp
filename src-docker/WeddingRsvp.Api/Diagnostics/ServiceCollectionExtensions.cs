using System.Reflection;
using OpenTelemetry.Resources;

namespace WeddingRsvp.Api.Diagnostics;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApiOpenTelemetry( this IServiceCollection services )
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
            .WithMetrics()
            .WithLogging()
            .WithTracing();

        return services;
    }
}