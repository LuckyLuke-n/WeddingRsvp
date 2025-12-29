using System.Globalization;

namespace WeddingRsvp.WebApp.Components.Helpers;

public static class CultureLinker
{
    public static void SetCulture(string cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName)) return;
        
        var culture = new CultureInfo(cultureName);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}