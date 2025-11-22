using MongoDB.Driver;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;

namespace WeddingRsvp.Api.Repository;

public class RsvpRepository : MongoDbRepository<Rsvp>, IRsvpRepository
{
    public RsvpRepository( IMongoClient mongoClient, ILogger<MongoDbRepository<Rsvp>> logger ) : base( mongoClient, logger )
    {
    }

    public override Task<RepositoryResponse<Rsvp, RepositoryFailResponse>> UpdateAsync(Rsvp entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}