using Microsoft.AspNetCore.Localization;

namespace WeddingRsvp.WebApp.Components.Helpers;

public class RouteCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value;
        if (string.IsNullOrWhiteSpace(path))
            return NullRack;

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 1)
        {
            var culture = segments[0];
            // Basic check to see if the first segment is 2 characters (e.g., 'en', 'de')
            if (culture.Length == 2)
                return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture));
        }

        return NullRack;
    }

    private static readonly Task<ProviderCultureResult?> NullRack = Task.FromResult<ProviderCultureResult?>(null);
}