namespace WeddingRsvp.Api.Repository;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddMongoDbRsvpRepostiroy(this IHostApplicationBuilder builder)
    {
        builder.AddMongoDBClient(connectionName: "weddingrsvp-mongo");
        builder.Services.AddScoped<IRsvpRepository, RsvpRepository>();
        
        return builder;
    }
}