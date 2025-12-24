namespace WeddingRsvp.Client.Configurations;

public class WeddingRsvpClientConfiguration
{
    public static string Section => "WeddingRsvpClient";
    
    public string ApiKey { get; set; } = string.Empty;
}