using System.Diagnostics.Metrics;

namespace WeddingRsvp.Api.Diagnostics.Meters;

public static class ResponseMeter
{
    public static string Name => "WeddingRsvpApi.Responses";
    private static Meter Meter { get; } = new(Name, "1.0");
    private static Counter<int> ResponseCounter { get; }
    
    static ResponseMeter()
    {
        // Initialize the metrics
        ResponseCounter = Meter.CreateCounter<int>( "response.count", "count", "The number of responses." );
    }

    internal static void CountResponse( string guest )
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("guest", guest),
        };
        
        ResponseCounter.Add( 1, tags );
    }
}