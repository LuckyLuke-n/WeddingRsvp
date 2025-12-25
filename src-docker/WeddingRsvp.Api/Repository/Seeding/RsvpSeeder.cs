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
                Id = "e69a7fd5-f76b-41b0-ab28-78a9b9a9ba2b",
                Name = "John and Jane",
                Language = Language.en,
            },

            new()
            {
                Id = "79d33d23-9f91-4fcb-a777-1c2cbad82a46",
                Name = "Maria",
                Language = Language.en,
            },

            new()
            {
                Id = "3b5382b1-5624-4cb2-928f-603356d63bdf",
                Name = "Max",
                Language = Language.de,
                Attending = Reply.Yes,
                BringPartner = Reply.Yes,
                NumberOfGuestOvernight = 2,
                NumberOfMeatMenus = 1,
                NumberOfFishMenus = 1,
                NumberOfVegetarianMenus = 0,
                AdditionalInformation = "Let's do this.",
                Salutation = "Lieber Max,",
            },
        ];

        await Collection.InsertManyAsync(rsvps, null, cancellationToken).ConfigureAwait(false);
        _logger.LogWarning("Seeding completed.");
    }
}