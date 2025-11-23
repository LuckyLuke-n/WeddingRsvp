using System.Net;
using MongoDB.Driver;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;

namespace WeddingRsvp.Api.Repository;

public class RsvpRepository : MongoDbRepository<Rsvp>, IRsvpRepository
{
    public RsvpRepository(IMongoClient mongoClient, ILogger<MongoDbRepository<Rsvp>> logger) : base(mongoClient, logger)
    {
    }

    public override async Task<RepositoryResponse<Rsvp, RepositoryFailResponse>> UpdateAsync(Rsvp entity,
        CancellationToken cancellationToken = default)
    {
        if (Collection is null)
            return NotConnectedFailedResponse();

        var filter = Builders<Rsvp>.Filter
            .Eq(r => r.Id, entity.Id);

        var update = Builders<Rsvp>.Update
            .Set(r => r.NumberOfGuests, entity.NumberOfGuests)
            .Set(r => r.NumberOfNormalMeals, entity.NumberOfNormalMeals)
            .Set(r => r.NumberOfVeganMeals, entity.NumberOfVeganMeals)
            .Set(r => r.NumberOfVegetarianMeals, entity.NumberOfVegetarianMeals)
            .Set(r => r.AdditionalInformation, entity.AdditionalInformation);

        try
        {
            var updated = await Collection.FindOneAndUpdateAsync<Rsvp>(filter, update, null, cancellationToken)
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