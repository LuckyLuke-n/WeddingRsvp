namespace WeddingRsvp.Abstractions.Models;

public class PostRsvpDto
{
    public string Name { get; set; } = string.Empty;
    public GuestType Type { get; set; }
    public int NumberOfGuests { get; set; }
}