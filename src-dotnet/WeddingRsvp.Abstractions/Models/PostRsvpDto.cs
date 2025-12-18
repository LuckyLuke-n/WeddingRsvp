using System.ComponentModel.DataAnnotations;

namespace WeddingRsvp.Abstractions.Models;

public class PostRsvpDto
{
    [Required]
    [Length(1, 255)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Length(1, 255)]
    public string Salutation { get; set; } = string.Empty;
    
    [Required]
    [EnumDataType(typeof(Language))]
    public Language Language { get; set; }
    
    [Required]
    public bool IsPlural { get; set; }
}