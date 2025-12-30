namespace WeddingRsvp.Abstractions.Models.Rsvps;

public class GetRsvpDto
{
    public string Id { get; set; } = string.Empty;
    public DateTime? LastUpdated { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string Salutation { get; set; } = string.Empty;
    public Language Language { get; set; }
    public bool IsPlural { get; set; }
    
    public Reply Attending { get; set; }
    public Reply BringPartner { get; set; }
    public int NumberOfGuestsOvernight { get; set; }
    
    public int NumberOfMeatMenus { get; set; }
    public int NumberOfFishMenus { get; set; }
    public int NumberOfVegetarianMenus { get; set; }
    
    public string AdditionalInformation { get; set; } = string.Empty;
}