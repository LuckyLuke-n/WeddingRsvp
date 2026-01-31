using WeddingRsvp.Abstractions.Models;
using WeddingRsvp.Abstractions.Models.Rsvps;

namespace WeddingRsvp.Client;

public class RsvpGuest
{
    public string Id { get; set; } = string.Empty;
    public ResponseType Response { get; set; }
    public ResponseType BringPartner { get; set; }
    public int NumberOfGuestsOvernight { get; set; }
    public int NumberOfBrunchGuests { get; set; }
    public int NumberOfMeatMenus { get; set; }
    public int NumberOfVegetarianMenus { get; set; }
    public string AdditionalInformation { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    public bool IsPlural { get; set; }
    public string Salutation { get; set; } = string.Empty;

    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        return HashCode.Combine(
            Name,
            Response,
            BringPartner,
            NumberOfGuestsOvernight,
            NumberOfBrunchGuests,
            NumberOfMeatMenus,
            NumberOfVegetarianMenus,
            AdditionalInformation
        );
    }

    public void UpdateForNotAttending()
    {
        BringPartner = Response;
        NumberOfGuestsOvernight = 0;
        NumberOfBrunchGuests = 0;
        NumberOfMeatMenus = 0;
        NumberOfVegetarianMenus = 0;
        AdditionalInformation = "";
    }
}