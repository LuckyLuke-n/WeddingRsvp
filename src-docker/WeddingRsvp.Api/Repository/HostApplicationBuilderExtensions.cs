namespace WeddingRsvp.Api.Repository;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddMongoDbRepostiroy(this IHostApplicationBuilder builder)
    {
        builder.AddMongoDBClient(connectionName: "rsvp-mongo");
        
        return builder;
    }
}