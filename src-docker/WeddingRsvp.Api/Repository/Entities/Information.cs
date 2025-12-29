namespace WeddingRsvp.Api.Repository.Entities;

public class Information : IEntity
{
    public string Id { get; set; } = string.Empty;
    
    public string Language { get; set; } = string.Empty;
    
    public string InvitationText { get; set; } = string.Empty;
    public List<ItineraryItem> Itinerary { get; set; } = [];
    public List<Faq> Faqs { get; set; } = [];
    
    public void SetAsNew() => Id = Guid.NewGuid().ToString();
}