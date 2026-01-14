namespace WeddingRsvp.Abstractions.Models.Information;

public class GetInformationDto
{
    public string Id { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string InvitationText { get; set; } = string.Empty;
    public Dictionary<string,string> Itinerary { get; set; } = [];
    public Dictionary<string,string> Faqs { get; set; } = [];
}