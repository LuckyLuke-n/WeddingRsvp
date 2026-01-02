using System.Net;
using MongoDB.Driver;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;

namespace WeddingRsvp.Api.Repository;

public class InformationRepository : MongoDbRepository<Information>, IInformationRepository
{
    public InformationRepository(IMongoClient mongoClient, ILogger<MongoDbRepository<Information>> logger) : base(mongoClient, logger)
    {
    }
    
    public override async Task<RepositoryResponse<Information, RepositoryFailResponse>> UpdateAsync(Information entity, CancellationToken cancellationToken = default)
    {
        if (Collection is null)
            return NotConnectedFailedResponse();

        var filter = Builders<Information>.Filter
            .Eq(r => r.Id, entity.Id);

        var update = Builders<Information>.Update
            .Set(r => r.Language, entity.Language)
            .Set(r => r.InvitationText, entity.InvitationText)
            .Set(r => r.Itinerary, entity.Itinerary)
            .Set(r => r.Faqs, entity.Faqs);

        var options = new FindOneAndUpdateOptions<Information>
        {
            ReturnDocument = ReturnDocument.After
        };

        try
        {
            var updated = await Collection.FindOneAndUpdateAsync<Information>(filter, update, options, cancellationToken)
                .ConfigureAwait(false);

            if (updated is not null)
                return RepositoryResponse<Information, RepositoryFailResponse>.CreateSuccess(updated);

            RepositoryFailResponse fail = new()
            {
                StatusCode = HttpStatusCode.NotFound, 
                Message = "Document cannot be updated. Document not found."
            };

            return RepositoryResponse<Information, RepositoryFailResponse>.CreateFail(fail);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error updating information in mongo.");
            RepositoryFailResponse fail = new()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = ex.Message,
            };
            return RepositoryResponse<Information, RepositoryFailResponse>.CreateFail(fail);
        }
    }
    
    public async Task<RepositoryResponse<Information, RepositoryFailResponse>> ReadByLanguageAsync(string language, CancellationToken cancellationToken = default)
    {
        if (Collection is null)
            return NotConnectedFailedResponse();

        try
        {
            var filter = Builders<Information>.Filter.Eq(r => r.Language, language);
            var result = await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (result is not null)
                return RepositoryResponse<Information, RepositoryFailResponse>.CreateSuccess(result);

            return RepositoryResponse<Information, RepositoryFailResponse>.CreateFail(new RepositoryFailResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = $"Information for language '{language}' not found."
            });
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error getting information by language from mongo.");
            return RepositoryResponse<Information, RepositoryFailResponse>.CreateFail(new RepositoryFailResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Overrides the base method to check if the language already exists. If the language already exists, a conflict is returned.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<RepositoryResponse<Information, RepositoryFailResponse>> CreateAsync(Information entity,
        CancellationToken cancellationToken = default)
    {
        if ( Collection is null )
            return RepositoryResponse<Information, RepositoryFailResponse>.CreateFail( new() { StatusCode = HttpStatusCode.InternalServerError, Message = "No connection to the mongo collection." } );
        
        entity.Language = entity.Language.ToLowerInvariant();
            
        var filter = Builders<Information>.Filter
            .Eq( d => d.Language, entity.Language );
        
        var cursor = await Collection.FindAsync<Information>( filter, null, cancellationToken ).ConfigureAwait( false );
        var documents = await cursor.ToListAsync( cancellationToken ).ConfigureAwait( false );

        if ( documents.Count > 0 )
        {
            RepositoryFailResponse fail = new() { StatusCode = HttpStatusCode.Conflict, Message = $"Information already exists for language {entity.Language}." };
            return RepositoryResponse<Information, RepositoryFailResponse>.CreateFail( fail );
        }
        
        return await base.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
    }
}