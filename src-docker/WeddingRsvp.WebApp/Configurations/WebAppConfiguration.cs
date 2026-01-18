namespace WeddingRsvp.WebApp.Configurations;

public class WebAppConfiguration
{
    public static string Section => "WeddingRsvpWebApp";
    
    public string AdminPassword { get; set; } = string.Empty;
}