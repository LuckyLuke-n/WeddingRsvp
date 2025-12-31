using WeddingRsvp.Abstractions.Models;
using WeddingRsvp.Abstractions.Models.Rsvps;

namespace WeddingRsvp.Client;

public class RsvpGuest
{
    public string Id { get; set; } = string.Empty;
    public ResponseType Response { get; set; }
    public ResponseType BringPartner { get; set; }
    public int NumberOfGuestsOvernight { get; set; }
    public int NumberOfMeatMenus { get; set; }
    public int NumberOfFishMenus { get; set; }
    public int NumberOfVegetarianMenus { get; set; }
    public string AdditionalInformation { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    public bool IsPlural { get; set; }
    public string Salutation { get; set; } = string.Empty;
}