using System.Globalization;
using Microsoft.AspNetCore.Localization;
using WeddingRsvp.Client;
using WeddingRsvp.WebApp.Components;
using WeddingRsvp.WebApp.Components.Helpers;
using WeddingRsvp.WebApp.Components.Pages;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddRsvpClient();

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Define supported cultures
var supportedCultures = SupportedCultures.Cultures;
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
localizationOptions.RequestCultureProviders.Clear();
localizationOptions.RequestCultureProviders.Insert(0,new RouteCultureProvider());
localizationOptions.RequestCultureProviders.Add(new CookieRequestCultureProvider());

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseRequestLocalization(localizationOptions);

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.Use((context, next) =>
{
    var requestCultureFeature = context.Features.Get<IRequestCultureFeature>();
    if (requestCultureFeature is not null)
    {
        var culture = requestCultureFeature.RequestCulture.Culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
    return next();
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

#if !DEBUG
app.UseOpenTelemetryPrometheusScrapingEndpoint();
#endif

app.Run();