namespace WeddingRsvp.Api.Services;

public class EmailTemplate
{
    public string Name { get; set; } = string.Empty;
    public bool Attending { get; set; }
    public bool BringPartner { get; set; }
    public int NumberOfGuestsOvernight { get; set; }
    public int NumberOfMeatMenus { get; set; }
    public int NumberOfVegetarianMenus { get; set; }
    public int NumberOfBrunchGuests { get; set; }
    public string AdditionalInformation { get; set; } = string.Empty;
}