using WeddingRsvp.Abstractions.Models;

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
            Language = (Abstractions.Models.Language)entity.Language,
            LastUpdated = entity.LastUpdated,
            Salutation = entity.Salutation,
            Attending =  (Abstractions.Models.Reply)entity.Attending,
            BringPartner = (Abstractions.Models.Reply)entity.BringPartner,
            IsPlural = entity.IsPlural,
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
}