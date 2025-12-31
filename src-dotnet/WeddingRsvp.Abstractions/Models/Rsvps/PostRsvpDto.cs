using System.ComponentModel.DataAnnotations;

namespace WeddingRsvp.Abstractions.Models.Rsvps;

public class PostRsvpDto
{
    [Required]
    [Length(1, 255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Length(1, 255)]
    public string Salutation { get; set; } = string.Empty;
    
    [Required]
    public bool IsPlural { get; set; }
}