namespace WeddingRsvp.Api.Services;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddEmailService(this IServiceCollection services)
    {
        return services.AddTransient<IEmailService, SendGridEmailService>();
    }
}