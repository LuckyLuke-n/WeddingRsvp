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
            NumberOfGuests = entity.NumberOfGuests,
            NumberOfGuestsAttending = entity.NumberOfGuestsAttending,
            NumberOfNormalMeals = entity.NumberOfNormalMeals,
            NumberOfVeganMeals = entity.NumberOfVeganMeals,
            NumberOfVegetarianMeals = entity.NumberOfVegetarianMeals,
            Type = (Abstractions.Models.GuestType)entity.Type,
        };
        
        return dto;
    }

    public static Rsvp ToEntity(this PostRsvpDto dto)
    {
        Rsvp rsvp = new()
        {
            Name = dto.Name,
            Type = (GuestType)dto.Type,
            NumberOfGuests = dto.NumberOfGuests,
        };
        
        return rsvp;
    }

    public static Rsvp ToEntity(this PutRsvpDto dto)
    {
        Rsvp entity = new()
        {
            AdditionalInformation = dto.AdditionalInformation,
            Name = dto.Name,
            NumberOfGuests = dto.NumberOfGuests,
            NumberOfGuestsAttending = dto.NumberOfGuestsAttending,
            NumberOfNormalMeals = dto.NumberOfNormalMeals,
            NumberOfVeganMeals = dto.NumberOfVeganMeals,
            NumberOfVegetarianMeals = dto.NumberOfVegetarianMeals,
            Type = (GuestType)dto.Type,
        };
        
        return entity;
    }
}