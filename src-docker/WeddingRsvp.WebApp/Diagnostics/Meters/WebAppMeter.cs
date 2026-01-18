using System.Diagnostics.Metrics;

namespace WeddingRsvp.WebApp.Diagnostics.Meters;

public static class WebAppMeter
{
    public static string Name => "WeddingRsvpWebApp";
    private static Meter Meter { get; } = new(Name, "1.0");
    private static Counter<int> AdminPageSuccessLogins { get; }
    private static Counter<int> AdminPageFailLogins { get; }
    
    static WebAppMeter()
    {
        // Initialize the metrics
        AdminPageSuccessLogins = Meter.CreateCounter<int>( "login.admin.success.count", "count", "The number of successful admin logins." );
        AdminPageFailLogins = Meter.CreateCounter<int>( "login.admin.fail.count", "count", "The number of failed admin logins." );
    }

    internal static void SuccessfulAdminLogin()
    {
        AdminPageSuccessLogins.Add( 1 );
    }
    
    internal static void FailedAdminLogin()
    {
        AdminPageFailLogins.Add( 1 );
    }
}