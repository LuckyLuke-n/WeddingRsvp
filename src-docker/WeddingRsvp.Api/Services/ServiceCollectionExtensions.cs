namespace WeddingRsvp.Api.Services;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddEmailService(this IServiceCollection services)
    {
        return services.AddSingleton<IEmailService, SendGridEmailService>();
    }
}