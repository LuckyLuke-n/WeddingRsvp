using MongoDB.Driver;
using WeddingRsvp.Api.Repository.Entities;

namespace WeddingRsvp.Api.Repository.Seeding;

public class RsvpSeeder
{
    private IMongoCollection<Rsvp>? RsvpCollection { get; }
    private IMongoCollection<Information>? InformationCollection { get; }
    private readonly ILogger<RsvpSeeder> _logger;

    public RsvpSeeder(IMongoClient mongoClient, ILogger<RsvpSeeder> logger)
    {
        _logger = logger;

        try
        {
            var database = mongoClient.GetDatabase("WeddingRsvp");
            RsvpCollection = database.GetCollection<Rsvp>(nameof(Rsvp));
            InformationCollection = database.GetCollection<Information>(nameof(Information));
        }
        catch (MongoException ex)
        {
            _logger.LogCritical(ex, "Mongo initialization for seeding failed.");
        }
    }


    public async Task RunAsync(bool onlyClean = false, CancellationToken cancellationToken = default)
    {
        if (RsvpCollection is null || InformationCollection is null)
        {
            _logger.LogError("Mongo db not initialized.");
            return;
        }

        await RsvpCollection.DeleteManyAsync(FilterDefinition<Rsvp>.Empty, cancellationToken).ConfigureAwait(false);
        await InformationCollection.DeleteManyAsync(FilterDefinition<Information>.Empty, cancellationToken)
            .ConfigureAwait(false);
        _logger.LogWarning("Cleaning mongo db completed.");
        
        if (onlyClean)
            return;

        Rsvp[] rsvps =
        [
            new()
            {
                Id = "e69a7fd5-f76b-41b0-ab28-78a9b9a9ba2b",
                Name = "John and Jane",
                IsPlural = true,
                Salutation = "Dear John and Jane,",
            },

            new()
            {
                Id = "79d33d23-9f91-4fcb-a777-1c2cbad82a46",
                Name = "Maria",
                Salutation = "Dear Maria,",
            },

            new()
            {
                Id = "3b5382b1-5624-4cb2-928f-603356d63bdf",
                Name = "Max",
                Attending = Reply.Yes,
                BringPartner = Reply.Yes,
                NumberOfGuestsOvernight = 2,
                NumberOfMeatMenus = 1,
                NumberOfFishMenus = 1,
                NumberOfVegetarianMenus = 0,
                AdditionalInformation = "Let's do this.",
                Salutation = "Lieber Max,",
            },
        ];

        Information[] information =
        [
            new()
            {
                Id = "cebf0267-8379-440d-a66b-b0ecdc1c0898",
                Language = "en",
                InvitationText = """
                                 We are overjoyed to invite you to celebrate our special day. 
                                 It wouldn't be the same without you there to share in our love and happiness as we start our new life together.
                                 """,
                Faqs = new List<Faq>
                {
                    new Faq { Question = "Is there a dress code?", Answer = "No." },
                    new Faq { Question = "Are children welcome?", Answer = "Yes, of course." },
                    new Faq
                    {
                        Question = "Where can I park?",
                        Answer = "There is ample parking available directly at the venue entrance."
                    }
                },
                Itinerary = new List<ItineraryItem>
                {
                    new ItineraryItem { Time = "14:00", Activity = "Welcome" },
                    new ItineraryItem { Time = "18:00", Activity = "Dinner" },
                    new ItineraryItem { Time = "15:00", Activity = "Coffee and cake" },
                    new ItineraryItem { Time = "20:00", Activity = "Dance" }
                }
            },
            new()
            {
                Id = "6d7c5bfa-e8bb-410e-b33b-87caedc95897",
                Language = "de",
                InvitationText = """
                                 Wir freuen uns riesig, euch zur Feier unseres besonderen Tages einzuladen.
                                 Es wäre nicht dasselbe ohne euch, um unsere Liebe und unser Glück zu teilen, 
                                 während wir unser gemeinsames Leben beginnen.
                                 """,
                Faqs = new List<Faq>
                {
                    new Faq { Question = "Gibt es einen Dresscode?", Answer = "Nein." },
                    new Faq { Question = "Sind Kinder willkommen?", Answer = "Ja, natürlich." },
                    new Faq
                    {
                        Question = "Wo kann ich parken?",
                        Answer = "Es gibt ausreichend Parkplätze direkt am Eingang des Veranstaltungsortes."
                    }
                },
                Itinerary = new List<ItineraryItem>
                {
                    new ItineraryItem { Time = "14:00", Activity = "Empfang" },
                    new ItineraryItem { Time = "15:00", Activity = "Kaffee und Kuchen" },
                    new ItineraryItem { Time = "18:00", Activity = "Abendessen" },
                    new ItineraryItem { Time = "20:00", Activity = "Tanz" }
                }
            }
        ];

        await RsvpCollection.InsertManyAsync(rsvps, null, cancellationToken).ConfigureAwait(false);
        await InformationCollection.InsertManyAsync(information, null, cancellationToken).ConfigureAwait(false);
        _logger.LogWarning("Seeding completed.");
    }
}