namespace WeddingRsvp.Api.Repository.Entities;

public class Rsvp : IEntity
{
    public string Id { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    public GuestType Type { get; set; }
    public int NumberOfGuests { get; set; }
    
    public int NumberOfNormalMeals { get; set; }
    public int NumberOfVegetarianMeals { get; set; }
    public int NumberOfVeganMeals { get; set; }
    
    public string AdditionalInformation { get; set; } = string.Empty;
    
    public void SetAsNew() => Id = Guid.NewGuid().ToString();
}