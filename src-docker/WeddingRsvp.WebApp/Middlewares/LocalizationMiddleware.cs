using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace WeddingRsvp.WebApp.Middlewares;

public class LocalizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var requestCultureFeature = context.Features.Get<IRequestCultureFeature>();

        if (requestCultureFeature != null)
        {
            var culture = requestCultureFeature.RequestCulture.Culture;
            var uiCulture = requestCultureFeature.RequestCulture.UICulture;

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = uiCulture;
        }

        await next(context);
    }
}