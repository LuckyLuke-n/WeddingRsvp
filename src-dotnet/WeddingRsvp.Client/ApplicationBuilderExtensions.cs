using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeddingRsvp.Client.Configurations;

namespace WeddingRsvp.Client;

public static class ApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddRsvpClient(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRsvpClient, WeddingRsvpClient>();
        builder.Services.AddScoped<IInformationClient, WeddingRsvpClient>();
        var config = builder.Configuration.GetSection(WeddingRsvpClientConfiguration.Section).Get<WeddingRsvpClientConfiguration>();
        
        builder.Services.AddHttpClient( WeddingRsvpClient.RsvpClientName, client =>
        {
            client.BaseAddress = new Uri("http://api/api/rsvps/"); 
            client.DefaultRequestHeaders.Add("x-api-key", config?.ApiKey ?? "");
        });
        builder.Services.AddHttpClient( WeddingRsvpClient.RsvpAdminClientName, client =>
        {
            client.BaseAddress = new Uri("http://api/api/rsvps/"); 
            client.DefaultRequestHeaders.Add("x-api-key", config?.ApiKey ?? "");
            client.DefaultRequestHeaders.Add("X-Auth-Admin", config?.AdminIdentifier ?? "");
        });
        builder.Services.AddHttpClient( WeddingRsvpClient.InformationClientName, client =>
        {
            client.BaseAddress = new Uri("http://api/api/informations/"); 
            client.DefaultRequestHeaders.Add("x-api-key", config?.ApiKey ?? "");
        });
        builder.Services.AddHttpClient( WeddingRsvpClient.InformationAdminClientName, client =>
        {
            client.BaseAddress = new Uri("http://api/api/informations/"); 
            client.DefaultRequestHeaders.Add("x-api-key", config?.ApiKey ?? "");
            client.DefaultRequestHeaders.Add("X-Auth-Admin", config?.AdminIdentifier ?? "");
        });
        
        return builder;
    }

    public static IHostApplicationBuilder AddNotificationClient(this IHostApplicationBuilder builder)
    {
        var config = builder.Configuration.GetSection(WeddingRsvpClientConfiguration.Section).Get<WeddingRsvpClientConfiguration>();
        builder.Services.AddScoped<INotificationClient, WeddingRsvpClient>();
        
        builder.Services.AddHttpClient( WeddingRsvpClient.NotificationClientName, client =>
        {
            client.BaseAddress = new Uri("http://api/api/notifications/"); 
            client.DefaultRequestHeaders.Add("x-api-key", config?.ApiKey ?? "");
        });
        
        return builder;
    }
}