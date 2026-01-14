namespace WeddingRsvp.Api.Configurations;

public class ApiConfiguration
{
    public static string Section => "WeddingRsvp";
    
    public string AdminIdentifier { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}