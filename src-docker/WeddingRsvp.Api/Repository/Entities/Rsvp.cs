namespace WeddingRsvp.Api.Repository.Entities;

public class Rsvp : IEntity
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public GuestType Type { get; set; }
    public int NumberOfGuests { get; set; }
    
    public int NumberOfNormalMeals { get; set; }
    public int NumberOfVegetarianMeals { get; set; }
    public int NumberOfVeganMeals { get; set; }
    
    public string AdditionalInformation { get; set; } = string.Empty;
    
    public void SetAsNew()
    {
        throw new NotImplementedException();
    }
}