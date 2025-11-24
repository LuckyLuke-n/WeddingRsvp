using System.ComponentModel.DataAnnotations;

namespace WeddingRsvp.Abstractions.Models;

public class PutRsvpDto
{
    [Length(1, 255)]
    public string Name { get; set; } = string.Empty;
    
    [EnumDataType(typeof(GuestType))]
    public GuestType Type { get; set; }
    
    [Range(1, 10)]
    public int NumberOfGuests { get; set; }
    
    [Required]
    [Range(1, 10)]
    public int NumberOfGuestsAttending { get; set; }
    
    [Required]
    [Range(1, 10)]
    public int NumberOfNormalMeals { get; set; }
    
    [Required]
    [Range(1, 10)]
    public int NumberOfVegetarianMeals { get; set; }
    
    [Required]
    [Range(1, 10)]
    public int NumberOfVeganMeals { get; set; }
    
    public string AdditionalInformation { get; set; } = string.Empty;
}