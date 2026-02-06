using System.Net;
using FluentAssertions;
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
        result.IsSuccess.Should().BeTrue();
        result.ValueSuccess.Should().BeEquivalentTo(info);

        var dbResponse = await _serviceUnderTest.ReadAsync(Guid.Parse(info.Id));
        dbResponse.IsSuccess.Should().BeTrue();
        dbResponse.ValueSuccess.Should().BeEquivalentTo(info);
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
        result.IsSuccess.Should().BeFalse();
        result.ValueFail.StatusCode.Should().Be(HttpStatusCode.Conflict);
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
        result.IsSuccess.Should().BeTrue();
        result.ValueSuccess.Should().BeEquivalentTo(info);
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnNotFound_WhenNotExists()
    {
        // Act
        var result = await _serviceUnderTest.ReadAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValueFail.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
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
        result.IsSuccess.Should().BeTrue();
        result.ValueSuccess.Should().BeEquivalentTo(info);
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
        result.IsSuccess.Should().BeFalse();
        result.ValueFail.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
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
        result.IsSuccess.Should().BeTrue();
        result.ValueSuccess.Should().ContainEquivalentOf(info1);
        result.ValueSuccess.Should().ContainEquivalentOf(info2);
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
        result.IsSuccess.Should().BeTrue();

        var readResult = await _serviceUnderTest.ReadAsync(Guid.Parse(info.Id));
        readResult.IsSuccess.Should().BeFalse();
        readResult.ValueFail.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
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
        result.IsSuccess.Should().BeTrue();
        result.ValueSuccess.Should().BeEquivalentTo(updated);

        var dbResponse = await _serviceUnderTest.ReadAsync(Guid.Parse(result.ValueSuccess.Id));
        dbResponse.IsSuccess.Should().BeTrue();
        dbResponse.ValueSuccess.Should().BeEquivalentTo(updated);
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
        result.IsSuccess.Should().BeFalse();
        result.ValueFail.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}