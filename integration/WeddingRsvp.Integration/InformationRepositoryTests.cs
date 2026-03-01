using System.Net;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Testcontainers.MongoDb;
using WeddingRsvp.Api.Repository;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;

namespace WeddingRsvp.Integration;

public class InformationRepositoryTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoDbContainer = new MongoDbBuilder("mongo:8").Build();
    private IInformationRepository _serviceUnderTest = null!;
    private IMongoClient _mongoClient = null!;

    public async Task InitializeAsync()
    {
        await _mongoDbContainer.StartAsync();
        var connectionString = _mongoDbContainer.GetConnectionString();
        _mongoClient = new MongoClient(connectionString);
        
        var loggerMock = new Mock<ILogger<MongoDbRepository<Information>>>();
        
        _serviceUnderTest = new InformationRepository(_mongoClient, loggerMock.Object);
    }

    public async Task DisposeAsync()
    {
        await _mongoDbContainer.StopAsync();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateInformation()
    {
        // Arrange
        var info = new Information
        {
            Language = "en",
            InvitationText = "Welcome to our wedding!",
            Faqs = new List<Faq> { new Faq { Question = "When?", Answer = "Now" } },
            Itinerary = new List<ItineraryItem> { new ItineraryItem { Activity = "Ceremony", Time = "14:00" } }
        };

        // Act
        var result = await _serviceUnderTest.CreateAsync(info);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(info, result.ValueSuccess);

        var dbResponse = await _serviceUnderTest.ReadAsync(Guid.Parse(info.Id));
        Assert.True(dbResponse.IsSuccess);
        Assert.Equivalent(info, dbResponse.ValueSuccess);
    }
    
    [Fact]
    public async Task CreateLanguateDuplicateAsync_ShouldReturnConflict()
    {
        // Arrange
        var info = new Information
        {
            Language = "en",
            InvitationText = "Welcome to our wedding!",
            Faqs = new List<Faq> { new Faq { Question = "When?", Answer = "Now" } },
            Itinerary = new List<ItineraryItem> { new ItineraryItem { Activity = "Ceremony", Time = "14:00" } }
        };
        var duplicate = new Information
        {
            Language = "en",
            InvitationText = "Welcome to our wedding!",
            Faqs = new List<Faq> { new Faq { Question = "When?", Answer = "Now" } },
            Itinerary = new List<ItineraryItem> { new ItineraryItem { Activity = "Ceremony", Time = "14:00" } }
        };

        // Act
        await _serviceUnderTest.CreateAsync(info);
        var result = await _serviceUnderTest.CreateAsync(duplicate);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Conflict, result.ValueFail.StatusCode);
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnInformation_WhenExists()
    {
        // Arrange
        var info = new Information
        {
            Language = "de",
            InvitationText = "Willkommen!"
        };
        await _serviceUnderTest.CreateAsync(info);

        // Act
        var result = await _serviceUnderTest.ReadAsync(Guid.Parse(info.Id));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(info, result.ValueSuccess);
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnNotFound_WhenNotExists()
    {
        // Act
        var result = await _serviceUnderTest.ReadAsync(Guid.NewGuid());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.ValueFail.StatusCode);
    }

    [Fact]
    public async Task ReadByLanguageAsync_ShouldReturnInformation_WhenLanguageExists()
    {
        // Arrange
        var language = "es";
        var info = new Information
        {
            Language = language,
            InvitationText = "¡Bienvenidos a nuestra boda!"
        };
        await _serviceUnderTest.CreateAsync(info);

        // Act
        var result = await _serviceUnderTest.ReadByLanguageAsync(language);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(info, result.ValueSuccess);
    }

    [Fact]
    public async Task ReadByLanguageAsync_ShouldReturnNotFound_WhenLanguageDoesNotExist()
    {
        // Arrange
        var language = "es";
        var info = new Information
        {
            Language = language,
            InvitationText = "¡Bienvenidos a nuestra boda!"
        };
        await _serviceUnderTest.CreateAsync(info);
        
        // Act
        var result = await _serviceUnderTest.ReadByLanguageAsync("it");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.ValueFail.StatusCode);
    }

    [Fact]
    public async Task ReadAllAsync_ShouldReturnAllInformation()
    {
        // Arrange
        var info1 = new Information { Language = "en", InvitationText = "Text 1" };
        var info2 = new Information { Language = "fr", InvitationText = "Text 2" };
        
        await _serviceUnderTest.CreateAsync(info1);
        await _serviceUnderTest.CreateAsync(info2);

        // Act
        var result = await _serviceUnderTest.ReadAllAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(result.ValueSuccess!, info => info.Language == info1.Language && info.InvitationText == info1.InvitationText);
        Assert.Contains(result.ValueSuccess!, info => info.Language == info2.Language && info.InvitationText == info2.InvitationText);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteInformation()
    {
        // Arrange
        var info = new Information { Language = "en", InvitationText = "To Delete" };
        await _serviceUnderTest.CreateAsync(info);

        // Act
        var result = await _serviceUnderTest.DeleteAsync(Guid.Parse(info.Id));

        // Assert
        Assert.True(result.IsSuccess);

        var readResult = await _serviceUnderTest.ReadAsync(Guid.Parse(info.Id));
        Assert.False(readResult.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, readResult.ValueFail.StatusCode);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldUpdateInformation()
    {
        // Arrange
        var original = new Information 
        {
            Language = "en",
            InvitationText = "Original Text",
        };
        var createResult = await _serviceUnderTest.CreateAsync(original);

        var updated = new Information
        {
            Id = createResult.ValueSuccess!.Id,
            Language = "en",
            InvitationText = "Updated Text",
            Faqs = new List<Faq> { new Faq { Question = "New Q", Answer = "New A" } }
        };

        // Act
        var result = await _serviceUnderTest.UpdateAsync(updated);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(updated, result.ValueSuccess);

        var dbResponse = await _serviceUnderTest.ReadAsync(Guid.Parse(result.ValueSuccess.Id));
        Assert.True(dbResponse.IsSuccess);
        Assert.Equivalent(updated, dbResponse.ValueSuccess);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        var updated = new Information
        {
            Id = Guid.NewGuid().ToString(),
            Language = "en",
            InvitationText = "Non-existent",
        };

        // Act
        var result = await _serviceUnderTest.UpdateAsync(updated);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.ValueFail.StatusCode);
    }
}
