namespace WeddingRsvp.Abstractions.Models.Settings;

public class GetSettingsDto
{
    public bool EnableEmailNotifications { get; set; } = true;
    public List<string> EmailRecipients { get; set; } = [];
    public DateTime RespondUntil { get; set; }
}