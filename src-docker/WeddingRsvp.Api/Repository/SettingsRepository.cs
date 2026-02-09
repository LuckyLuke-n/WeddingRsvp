using System.Net;
using MongoDB.Driver;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;

namespace WeddingRsvp.Api.Repository;

public class SettingsRepository : MongoDbRepository<Settings>, ISettingsRepository
{
    public SettingsRepository(IMongoClient mongoClient, ILogger<MongoDbRepository<Settings>> logger) : base(mongoClient,
        logger)
    {
    }
    
    public override async Task<RepositoryResponse<Settings, RepositoryFailResponse>> CreateAsync( Settings entity, CancellationToken cancellationToken = default )
    {
        if ( Collection is null )
            return NotConnectedFailedResponse();

        try
        {
            await Collection.InsertOneAsync( entity, null, cancellationToken ).ConfigureAwait( false );
        }
        catch ( Exception ex )
        {
            Logger.LogCritical( ex, "Error writing to mongo." );
            RepositoryFailResponse fail = new()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = ex.Message,
            };
            return RepositoryResponse<Settings,RepositoryFailResponse>.CreateFail( fail );
        }

        return RepositoryResponse<Settings, RepositoryFailResponse>.CreateSuccess( entity );
    }

    public override async Task<RepositoryResponse<Settings, RepositoryFailResponse>> UpdateAsync(Settings entity,
        CancellationToken cancellationToken = default)
    {
        if (Collection is null)
            return NotConnectedFailedResponse();

        var filter = Builders<Settings>.Filter
            .Eq(r => r.Id, entity.Id);

        var update = Builders<Settings>.Update
            .Set(r => r.EnableEmailNotifications, entity.EnableEmailNotifications)
            .Set(r => r.EmailRecipients, entity.EmailRecipients)
            .Set(r => r.RespondUntil, entity.RespondUntil);

        var options = new FindOneAndUpdateOptions<Settings>
        {
            ReturnDocument = ReturnDocument.After,
            IsUpsert = true,
        };

        try
        {
            var updated = await Collection.FindOneAndUpdateAsync<Settings>(filter, update, options, cancellationToken)
                .ConfigureAwait(false);

            if (updated is not null)
                return RepositoryResponse<Settings, RepositoryFailResponse>.CreateSuccess(updated);

            RepositoryFailResponse fail = new()
                { StatusCode = HttpStatusCode.NotFound, Message = "Document cannot be updated." };

            return RepositoryResponse<Settings, RepositoryFailResponse>.CreateFail(fail);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error reading from mongo.");
            RepositoryFailResponse fail = new()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = ex.Message,
            };
            return RepositoryResponse<Settings, RepositoryFailResponse>.CreateFail(fail);
        }
    }
}