using Microsoft.AspNetCore.Localization;

namespace WeddingRsvp.WebApp.Components.Helpers;

public class RouteCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value;
        if (string.IsNullOrWhiteSpace(path))
            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult("en"));
        
        if (path.Contains("/_blazor") || path.Contains("/_framework"))
            return Task.FromResult<ProviderCultureResult?>(null); // Let the CookieProvider handle it!

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 1)
        {
            var culture = segments[0];
            // Basic check to see if the first segment is 2 characters (e.g., 'en', 'de')
            if (culture.Length == 2)
                return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture));
        }

        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult("en"));
    }
}