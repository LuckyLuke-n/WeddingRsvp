using System.ComponentModel.DataAnnotations;

namespace WeddingRsvp.Abstractions.Models.Information;

public class PostInformationDto
{
    [Required]
    [Length(2,2)]
    public string Language { get; set; } = string.Empty;
    
    [Required]
    public string InvitationText { get; set; } = string.Empty;
    
    [Required]
    [MinLength(1, ErrorMessage = "The Itinerary must contain at least one item.")]
    public Dictionary<string,string> Itinerary { get; set; } = [];
    
    [Required]
    [MinLength(1, ErrorMessage = "The Faqs must contain at least one item.")]
    public Dictionary<string,string> Faqs { get; set; } = [];
}