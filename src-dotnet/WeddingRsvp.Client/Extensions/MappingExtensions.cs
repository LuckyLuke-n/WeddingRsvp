using WeddingRsvp.Abstractions.Models.Information;
using WeddingRsvp.Abstractions.Models.Notifications;
using WeddingRsvp.Abstractions.Models.Rsvps;
using WeddingRsvp.Abstractions.Models.Settings;

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
            NumberOfBrunchGuests = dto.NumberOfBrunchGuests,
            NumberOfVegetarianMenus = dto.NumberOfVegetarianMenus,
            AdditionalInformation = dto.AdditionalInformation,
            Name = dto.Name,
            Salutation = dto.Salutation,
            IsPlural = dto.IsPlural,
        };
    }

    internal static PutRsvpDto ToPutDto(this RsvpGuest rsvp)
    {
        return new PutRsvpDto()
        {
            Attending = (Reply)rsvp.Response,
            BringPartner = (Reply)rsvp.BringPartner,
            NumberOfGuestsOvernight = rsvp.NumberOfGuestsOvernight,
            NumberOfMeatMenus = rsvp.NumberOfMeatMenus,
            NumberOfBrunchGuests = rsvp.NumberOfBrunchGuests,
            NumberOfVegetarianMenus = rsvp.NumberOfVegetarianMenus,
            AdditionalInformation = rsvp.AdditionalInformation,
            Name = rsvp.Name,
            Salutation = rsvp.Salutation,
            IsPlural = rsvp.IsPlural,
        };
    }

    internal static PostRsvpDto ToPostDto(this RsvpGuest rsvp)
    {
        return new PostRsvpDto()
        {
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

    internal static PostInformationDto ToPostDto(this DynamicInformation information)
    {
        return new PostInformationDto()
        {
            Language = information.Language,
            InvitationText = information.InvitationText,
            Itinerary = information.Itinerary,
            Faqs = information.Faqs,
        };
    }

    internal static PutInformationDto ToPutDto(this DynamicInformation information)
    {
        return new PutInformationDto()
        {
            Language = information.Language,
            InvitationText = information.InvitationText,
            Itinerary = information.Itinerary,
            Faqs = information.Faqs,
        };
    }

    internal static PostEmailDto ToPostNotificationDto(this RsvpGuest rsvp)
    {
        return new PostEmailDto()
        {
            Name = rsvp.Name,
            Attending = rsvp.Response.ToString(),
            BringPartner = rsvp.BringPartner.ToString(),
            NumberOfGuestsOvernight = rsvp.NumberOfGuestsOvernight,
            NumberOfBrunchGuests = rsvp.NumberOfBrunchGuests,
            NumberOfMeatMenus = rsvp.NumberOfMeatMenus,
            NumberOfVegetarianMenus = rsvp.NumberOfVegetarianMenus,
            AdditionalInformation = rsvp.AdditionalInformation,
        };
    }

    internal static ApplicationSettings ToDomainObject(this GetSettingsDto dto)
    {
        return new ApplicationSettings()
        {
            EnableEmailNotifications = dto.EnableEmailNotifications,
            EmailRecipients = dto.EmailRecipients,
            RespondUntil = dto.RespondUntil,
        };
    }

    internal static PutSettingsDto ToPutDto(this ApplicationSettings settings)
    {
        return new PutSettingsDto()
        {
            EnableEmailNotifications = settings.EnableEmailNotifications,
            EmailRecipients = settings.EmailRecipients,
            RespondUntil = settings.RespondUntil,
        };
    }
}