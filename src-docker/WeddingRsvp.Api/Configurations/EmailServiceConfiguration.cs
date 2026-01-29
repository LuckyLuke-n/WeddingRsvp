namespace WeddingRsvp.Api.Configurations;

public class EmailServiceConfiguration
{
    public static string Section => "EmailService";
    
    public bool Enabled { get; set; } = false;
    public string ApiKey { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public string[] ToEmails { get; set; } = [];
    
}