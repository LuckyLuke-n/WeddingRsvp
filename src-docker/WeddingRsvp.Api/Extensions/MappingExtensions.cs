using WeddingRsvp.Abstractions.Models.Rsvps;
using WeddingRsvp.Api.Services;

namespace WeddingRsvp.Api.Extensions;

public static class MappingExtensions
{
    public static EmailTemplate ToEmailTemplate(this PutRsvpDto dto)
    {
        return new EmailTemplate()
        {
            Name = dto.Name,
            Attending = dto.Attending.ToString(),
            BringPartner = dto.BringPartner.ToString(),
            NumberOfGuestsOvernight = dto.NumberOfGuestsOvernight,
            NumberOfMeatMenus = dto.NumberOfMeatMenus,
            NumberOfVegetarianMenus = dto.NumberOfVegetarianMenus,
            NumberOfBrunchGuests = 0,
            AdditionalInformation = dto.AdditionalInformation,
        };
    }
}