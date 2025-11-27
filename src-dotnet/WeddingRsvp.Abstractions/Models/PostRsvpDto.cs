using System.ComponentModel.DataAnnotations;

namespace WeddingRsvp.Abstractions.Models;

public class PostRsvpDto
{
    [Required]
    [Length(1, 255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EnumDataType(typeof(GuestType))]
    public GuestType Type { get; set; }
    
    [Required]
    [EnumDataType(typeof(Language))]
    public Language Language { get; set; }
    
    [Required]
    [Range(1, 10)]
    public int NumberOfGuests { get; set; }
}