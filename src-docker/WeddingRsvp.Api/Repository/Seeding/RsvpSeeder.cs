using MongoDB.Driver;
using WeddingRsvp.Api.Repository.Entities;

namespace WeddingRsvp.Api.Repository.Seeding;

public class RsvpSeeder
{
    public IMongoCollection<Rsvp>? Collection { get; }
    private readonly ILogger<RsvpSeeder> _logger;

    public RsvpSeeder(IMongoClient mongoClient, ILogger<RsvpSeeder> logger)
    {
        _logger = logger;

        try
        {
            var database = mongoClient.GetDatabase("Rsvp");
            Collection = database.GetCollection<Rsvp>(nameof(Rsvp));
        }
        catch (MongoException ex)
        {
            _logger.LogCritical(ex, "Mongo initialization for seeding failed.");
        }
    }


    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (Collection is null)
        {
            _logger.LogError("Mongo db not initialized.");
            return;
        }

        await Collection.DeleteManyAsync(FilterDefinition<Rsvp>.Empty, cancellationToken).ConfigureAwait(false);
        _logger.LogWarning("Cleaning mongo db completed.");

        Rsvp[] rsvps =
        [
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "John and Jane",
                Language = Language.en,
            },

            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Maria",
                Language = Language.en,
            },

            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Max",
                Language = Language.de,
            },
        ];

        await Collection.InsertManyAsync(rsvps, null, cancellationToken).ConfigureAwait(false);
        _logger.LogWarning("Seeding completed.");
    }
}