using WeddingRsvp.Abstractions.Models.Notifications;
using WeddingRsvp.Api.Services;

namespace WeddingRsvp.Api.Extensions;

public static class MappingExtensions
{
    public static EmailTemplate ToEmailTemplate(this PostEmailDto dto)
    {
        return new EmailTemplate()
        {
            Name = dto.Name,
            Attending = dto.Attending,
            BringPartner = dto.BringPartner,
            NumberOfGuestsOvernight = dto.NumberOfGuestsOvernight,
            NumberOfMeatMenus = dto.NumberOfMeatMenus,
            NumberOfVegetarianMenus = dto.NumberOfVegetarianMenus,
            NumberOfBrunchGuests = 0,
            AdditionalInformation = dto.AdditionalInformation,
        };
    }
}