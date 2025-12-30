using WeddingRsvp.Abstractions.Models;
using WeddingRsvp.Abstractions.Models.Information;
using WeddingRsvp.Abstractions.Models.Rsvps;

namespace WeddingRsvp.Api.Repository.Entities;

public static class MappingExtensions
{
    public static GetRsvpDto ToDto(this Rsvp entity)
    {
        GetRsvpDto dto = new()
        {
            AdditionalInformation = entity.AdditionalInformation,
            Id = entity.Id,
            Name = entity.Name,
            NumberOfGuestsOvernight = entity.NumberOfGuestsOvernight,
            NumberOfMeatMenus = entity.NumberOfMeatMenus,
            NumberOfVegetarianMenus = entity.NumberOfVegetarianMenus,
            NumberOfFishMenus = entity.NumberOfFishMenus,
            Language = (Abstractions.Models.Rsvps.Language)entity.Language,
            LastUpdated = entity.LastUpdated,
            Salutation = entity.Salutation,
            Attending =  (Abstractions.Models.Rsvps.Reply)entity.Attending,
            BringPartner = (Abstractions.Models.Rsvps.Reply)entity.BringPartner,
            IsPlural = entity.IsPlural,
        };
        
        return dto;
    }

    public static GetInformationDto ToDto(this Information entity)
    {
        GetInformationDto dto = new()
        {
            Id = entity.Id,
            Language = entity.Language,
            InvitationText = entity.InvitationText,
            Itinerary = entity.Itinerary.ToDictionary(x => x.Time, x => x.Activity),
            Faqs = entity.Faqs.ToDictionary(x => x.Question, x => x.Answer)
        };

        return dto;
    }

    public static Rsvp ToEntity(this PostRsvpDto dto)
    {
        Rsvp rsvp = new()
        {
            Name = dto.Name,
            Salutation = dto.Salutation,
            Language = (Language)dto.Language,
            IsPlural = dto.IsPlural,
        };
        
        return rsvp;
    }

    public static Rsvp ToEntity(this PutRsvpDto dto)
    {
        Rsvp entity = new()
        {
            Name = dto.Name,
            Salutation = dto.Salutation,
            Attending = (Reply)dto.Attending,
            BringPartner = (Reply)dto.BringPartner,
            Language = (Language)dto.Language,
            NumberOfMeatMenus = dto.NumberOfMeatMenus,
            NumberOfVegetarianMenus = dto.NumberOfVegetarianMenus,
            NumberOfFishMenus = dto.NumberOfFishMenus,
            NumberOfGuestsOvernight = dto.NumberOfGuestsOvernight,
            AdditionalInformation = dto.AdditionalInformation,
            LastUpdated = DateTime.UtcNow,
            IsPlural = dto.IsPlural,
        };
        
        return entity;
    }

    public static Information ToEntity(this PostInformationDto dto)
    {
        List<Faq> faqs = [];
        List<ItineraryItem> itinerary = [];
        
        foreach (var faq in dto.Faqs)
            faqs.Add(new Faq { Question = faq.Key, Answer = faq.Value });
        
        foreach (var item in dto.Itinerary)
            itinerary.Add(new ItineraryItem { Activity = item.Value, Time = item.Key });

        Information information = new()
        {
            Language = dto.Language,
            InvitationText = dto.InvitationText,
            Itinerary = itinerary,
            Faqs = faqs,
        };
        
        return information;
    }
    
    public static Information ToEntity(this PutInformationDto dto)
    {
        List<Faq> faqs = [];
        List<ItineraryItem> itinerary = [];
        
        foreach (var faq in dto.Faqs)
            faqs.Add(new Faq { Question = faq.Key, Answer = faq.Value });
        
        foreach (var item in dto.Itinerary)
            itinerary.Add(new ItineraryItem { Activity = item.Value, Time = item.Key });

        Information information = new()
        {
            Language = dto.Language,
            InvitationText = dto.InvitationText,
            Itinerary = itinerary,
            Faqs = faqs,
        };
        
        return information;
    }
}