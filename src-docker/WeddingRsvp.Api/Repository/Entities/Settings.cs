namespace WeddingRsvp.Api.Repository.Entities;

public class Settings : IEntity
{
    public string Id { get; set; } = string.Empty;
    
    public bool EnableEmailNotifications { get; set; } = true;
    public List<string> EmailRecipients { get; set; } = [];
    
    public DateTime RespondUntil { get; set; } = DateTime.UtcNow.AddDays(14);
    
    public void SetAsNew() => Id = Guid.NewGuid().ToString();
}