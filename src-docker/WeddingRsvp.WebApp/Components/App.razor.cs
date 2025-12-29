using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Localization;

namespace WeddingRsvp.WebApp.Components;

public partial class App
{
    [CascadingParameter]
    public HttpContext? HttpContext { get; set; }

    protected override void OnInitialized()
    {
        var currentCulture = CultureInfo.CurrentCulture.Name;

        if (HttpContext is not null)
        {
            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(currentCulture));
            var existingCookie = HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];

            if (existingCookie != cookieValue)
            {
                HttpContext.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    cookieValue,
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), Path = "/" }
                );
            }
        }
    }
}