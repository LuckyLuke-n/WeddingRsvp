using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi;
using WeddingRsvp.Api.Configurations;
using WeddingRsvp.Api.Repository;
using WeddingRsvp.Api.Security;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.Configure<ApiConfiguration>( builder.Configuration.GetSection( ApiConfiguration.Section ) );
builder.Services.AddSingleton(TimeProvider.System);
builder.AddMongoDbRsvpRepostiroy();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( options =>
{
    options.SwaggerDoc( "v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "WeddingRsvp API",
        Description = "An ASP.NET Core Web API for managing the wedding RSVPs.",
        License = new OpenApiLicense
        {
            Name = "BSD-3-Clause",
#pragma warning disable S1075 // URIs should not be hardcoded
            Url = new Uri( "https://opensource.org/licenses/BSD-3-Clause" )
#pragma warning restore S1075 // URIs should not be hardcoded
        }
    } );
} );

builder.Services.AddAuthentication( ApiKeyAuthenticationHandler.SchemeName )
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>( ApiKeyAuthenticationHandler.SchemeName, null );

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ApiKeyPolicy", policy =>
        policy.AddAuthenticationSchemes(ApiKeyAuthenticationHandler.SchemeName)
            .RequireAuthenticatedUser());

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() )
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#if !DEBUG
app.UseOpenTelemetryPrometheusScrapingEndpoint();
#endif

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}