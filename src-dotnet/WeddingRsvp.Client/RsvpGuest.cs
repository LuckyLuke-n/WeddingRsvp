using WeddingRsvp.Abstractions.Models;

namespace WeddingRsvp.Client;

public class RsvpGuest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ResponseType Response { get; set; }
    public ResponseType BringPartner { get; set; }
    public int NumberOfGuestsOvernight { get; set; }
    public int NumberOfNormalMeals { get; set; }
    public int NumberOfVegetarianMeals { get; set; }
    public int NumberOfVeganMeals { get; set; }
    public string AdditionalInformation { get; set; } = string.Empty;
}