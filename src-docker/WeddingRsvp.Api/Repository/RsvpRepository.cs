using System.Net;
using MongoDB.Driver;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;

namespace WeddingRsvp.Api.Repository;

public class RsvpRepository : MongoDbRepository<Rsvp>, IRsvpRepository
{
    private TimeProvider TimeProvider { get; }

    public RsvpRepository(IMongoClient mongoClient, TimeProvider timeProvider, ILogger<MongoDbRepository<Rsvp>> logger) : base(mongoClient, logger)
    {
        TimeProvider = timeProvider;
    }

    public override async Task<RepositoryResponse<Rsvp, RepositoryFailResponse>> UpdateAsync(Rsvp entity,
        CancellationToken cancellationToken = default)
    {
        if (Collection is null)
            return NotConnectedFailedResponse();

        var filter = Builders<Rsvp>.Filter
            .Eq(r => r.Id, entity.Id);

        var update = Builders<Rsvp>.Update
            .Set(r => r.LastUpdated, TimeProvider.GetUtcNow().UtcDateTime)
            .Set(r => r.Name, entity.Name)
            .Set(r => r.Language, entity.Language)
            .Set(r => r.NumberOfGuestOvernight, entity.NumberOfGuestOvernight)
            .Set(r => r.NumberOfMeatMenus, entity.NumberOfMeatMenus)
            .Set(r => r.NumberOfVegetarianMenus, entity.NumberOfVegetarianMenus)
            .Set(r => r.NumberOfFishMenus, entity.NumberOfFishMenus)
            .Set(r => r.AdditionalInformation, entity.AdditionalInformation)
            .Set(r => r.Attending, entity.Attending)
            .Set(r => r.BringPartner, entity.BringPartner)
            .Set(r => r.Salutation, entity.Salutation);

        var options = new FindOneAndUpdateOptions<Rsvp>
        {
            ReturnDocument = ReturnDocument.After
        };

        try
        {
            var updated = await Collection.FindOneAndUpdateAsync<Rsvp>(filter, update, options, cancellationToken)
                .ConfigureAwait(false);

            if (updated is not null)
                return RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateSuccess(updated);

            RepositoryFailResponse fail = new()
                { StatusCode = HttpStatusCode.NotFound, Message = "Document cannot be updated. Document not found." };

            return RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateFail(fail);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error reading from mongo.");
            RepositoryFailResponse fail = new()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = ex.Message,
            };
            return RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateFail(fail);
        }
    }
}