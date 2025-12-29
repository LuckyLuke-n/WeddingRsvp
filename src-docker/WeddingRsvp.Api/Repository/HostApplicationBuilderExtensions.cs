using WeddingRsvp.Api.Repository.Seeding;

namespace WeddingRsvp.Api.Repository;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddMongoDbRsvpRepository(this IHostApplicationBuilder builder)
    {
        builder.AddMongoDBClient(connectionName: "weddingrsvp-mongo");
        builder.Services.AddScoped<IRsvpRepository, RsvpRepository>();
        builder.Services.AddScoped<IInformationRepository, InformationRepository>();

        builder.Services.AddScoped<RsvpSeeder>();
        
        return builder;
    }
}