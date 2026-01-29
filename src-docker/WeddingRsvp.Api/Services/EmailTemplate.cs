namespace WeddingRsvp.Api.Services;

public class EmailTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Attending { get; set; } = string.Empty;
    public string BringPartner { get; set; } = string.Empty;
    public int NumberOfGuestsOvernight { get; set; }
    public int NumberOfMeatMenus { get; set; }
    public int NumberOfVegetarianMenus { get; set; }
    public int NumberOfBrunchGuests { get; set; }
    public string AdditionalInformation { get; set; } = string.Empty;
}