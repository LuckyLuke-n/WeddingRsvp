using WeddingRsvp.Api.Repository.Seeding;
using WeddingRsvp.Api.Services;

namespace WeddingRsvp.Api.Repository;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddMongoDbRsvpRepository(this IHostApplicationBuilder builder)
    {
        builder.AddMongoDBClient(connectionName: "weddingrsvp-mongo");
        builder.Services.AddScoped<IRsvpRepository, RsvpRepository>();
        builder.Services.AddScoped<IInformationRepository, InformationRepository>();
        builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
        builder.Services.AddScoped<ISettingsService, SettingsService>();

        builder.Services.AddScoped<RsvpSeeder>();
        
        return builder;
    }
}