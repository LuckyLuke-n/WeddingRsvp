namespace WeddingRsvp.Client;

public class ApplicationSettings
{
    public bool EnableEmailNotifications { get; set; } = true;
    public List<string> EmailRecipients { get; set; } = [];
    public DateTime RespondUntil { get; set; }
}