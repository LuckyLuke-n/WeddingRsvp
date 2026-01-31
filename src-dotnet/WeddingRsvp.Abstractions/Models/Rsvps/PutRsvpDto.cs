using System.ComponentModel.DataAnnotations;

namespace WeddingRsvp.Abstractions.Models.Rsvps;

public class PutRsvpDto
{
    [Required]
    [Length(1, 255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Length(1, 255)]
    public string Salutation { get; set; } = string.Empty;
    
    [Required]
    [EnumDataType(typeof(Reply))]
    public Reply Attending { get; set; }
    
    [Required]
    [EnumDataType(typeof(Reply))]
    public Reply BringPartner { get; set; }
    
    [Required]
    [Range(0, 10)]
    public int NumberOfGuestsOvernight { get; set; }
    
    [Required]
    [Range(0, 10)]
    public int NumberOfMeatMenus { get; set; }
    
    [Required]
    [Range(0, 10)]
    public int NumberOfBrunchGuests { get; set; }
    
    [Required]
    [Range(0, 10)]
    public int NumberOfVegetarianMenus { get; set; }
    
    public string AdditionalInformation { get; set; } = string.Empty;
    
    [Required]
    public bool IsPlural { get; set; }
}