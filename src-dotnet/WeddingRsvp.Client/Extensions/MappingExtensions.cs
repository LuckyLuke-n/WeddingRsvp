using WeddingRsvp.Abstractions.Models;

namespace WeddingRsvp.Client.Extensions;

internal static class MappingExtensions
{
    internal static RsvpGuest ToDomainObject(this GetRsvpDto dto)
    {
        return new RsvpGuest
        {
            Id = dto.Id,
            Response = (ResponseType)dto.Attending,
            BringPartner = (ResponseType)dto.BringPartner,
            NumberOfGuestsOvernight = dto.NumberOfGuestOvernight,
            NumberOfMeatMenus = dto.NumberOfMeatMenus,
            NumberOfFishMenus = dto.NumberOfFishMenus,
            NumberOfVegetarianMenus = dto.NumberOfVegetarianMenus,
            AdditionalInformation = dto.AdditionalInformation
        };
    }
}