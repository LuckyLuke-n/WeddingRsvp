using WeddingRsvp.Abstractions.Models.Information;
using WeddingRsvp.Abstractions.Models.Rsvps;

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
            NumberOfGuestsOvernight = dto.NumberOfGuestsOvernight,
            NumberOfMeatMenus = dto.NumberOfMeatMenus,
            NumberOfFishMenus = dto.NumberOfFishMenus,
            NumberOfVegetarianMenus = dto.NumberOfVegetarianMenus,
            AdditionalInformation = dto.AdditionalInformation,
            Name = dto.Name,
            Salutation = dto.Salutation,
            IsPlural = dto.IsPlural,
        };
    }

    internal static PutRsvpDto ToDto(this RsvpGuest rsvp)
    {
        return new PutRsvpDto()
        {
            Attending = (Reply)rsvp.Response,
            BringPartner = (Reply)rsvp.BringPartner,
            NumberOfGuestsOvernight = rsvp.NumberOfGuestsOvernight,
            NumberOfMeatMenus = rsvp.NumberOfMeatMenus,
            NumberOfFishMenus = rsvp.NumberOfFishMenus,
            NumberOfVegetarianMenus = rsvp.NumberOfVegetarianMenus,
            AdditionalInformation = rsvp.AdditionalInformation,
            Name = rsvp.Name,
            Salutation = rsvp.Salutation,
            IsPlural = rsvp.IsPlural,
        };
    }

    internal static DynamicInformation ToDomainObject(this GetInformationDto dto)
    {
        return new DynamicInformation()
        {
            Id = dto.Id,
            Language = dto.Language,
            InvitationText = dto.InvitationText,
            Itinerary = dto.Itinerary,
            Faqs = dto.Faqs,
        };
    }

    internal static PostInformationDto ToDto(this DynamicInformation information)
    {
        return new PostInformationDto()
        {
            Language = information.Language,
            InvitationText = information.InvitationText,
            Itinerary = information.Itinerary,
            Faqs = information.Faqs,
        };
    }
}