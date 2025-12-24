using System.Globalization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Testcontainers.MongoDb;
using WeddingRsvp.Api.Repository;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;

namespace WeddingRsvp.Integration;

public class RsvpRepositoryTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoDbContainer = new MongoDbBuilder().Build();
    private IRsvpRepository _serviceUnderTest = null!;
    private IMongoClient _mongoClient = null!;
    private TimeProvider TimeProvider { get; set; }

    public async Task InitializeAsync()
    {
        await _mongoDbContainer.StartAsync();
        var connectionString = _mongoDbContainer.GetConnectionString();
        _mongoClient = new MongoClient(connectionString);
        
        var loggerMock = new Mock<ILogger<MongoDbRepository<Rsvp>>>();
        
        var timeProviderMock = new Mock<TimeProvider>();
        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2022, 1, 1, 12, 0, 0, TimeSpan.Zero));
        TimeProvider = timeProviderMock.Object;
        
        _serviceUnderTest = new RsvpRepository(_mongoClient, TimeProvider, loggerMock.Object);
    }

    public async Task DisposeAsync()
    {
        await _mongoDbContainer.StopAsync();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateRsvp()
    {
        // Arrange
        var rsvp = new Rsvp
        {
            Language = Language.en,
            Name = "John Doe",
            NumberOfGuestOvernight = 2,
            NumberOfMeatMenus = 1,
            NumberOfFishMenus = 1,
            NumberOfVegetarianMenus = 0,
            AdditionalInformation = ""
        };

        // Act
        var result = await _serviceUnderTest.CreateAsync(rsvp);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ValueSuccess.Should().BeEquivalentTo(rsvp);

        var dbRsvpResponse = await _serviceUnderTest.ReadAsync(Guid.Parse(rsvp.Id));
        dbRsvpResponse.IsSuccess.Should().BeTrue();
        dbRsvpResponse.ValueSuccess.Should().BeEquivalentTo(rsvp);
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnRsvp_WhenExists()
    {
        // Arrange
        var rsvp = new Rsvp
        {
            Name = "Jane Doe",
            NumberOfGuestOvernight = 1,
            Language = Language.en,
        };
        await _serviceUnderTest.CreateAsync(rsvp);

        // Act
        var result = await _serviceUnderTest.ReadAsync(Guid.Parse(rsvp.Id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ValueSuccess.Should().BeEquivalentTo(rsvp);
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
    public async Task ReadAllAsync_ShouldReturnAllRsvps()
    {
        // Arrange
        var rsvp1 = new Rsvp { Name = "One" };
        var rsvp2 = new Rsvp { Name = "Two" };
        
        await _serviceUnderTest.CreateAsync(rsvp1);
        await _serviceUnderTest.CreateAsync(rsvp2);

        // Act
        var result = await _serviceUnderTest.ReadAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ValueSuccess.Should().ContainEquivalentOf(rsvp1);
        result.ValueSuccess.Should().ContainEquivalentOf(rsvp2);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteRsvp()
    {
        // Arrange
        var rsvp = new Rsvp { Id = Guid.NewGuid().ToString(), Name = "To Delete" };
        await _serviceUnderTest.CreateAsync(rsvp);

        // Act
        var result = await _serviceUnderTest.DeleteAsync(Guid.Parse(rsvp.Id));

        // Assert
        result.IsSuccess.Should().BeTrue();

        var readResult = await _serviceUnderTest.ReadAsync(Guid.Parse(rsvp.Id));
        readResult.IsSuccess.Should().BeFalse();
        readResult.ValueFail.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldUpdateRsvp()
    {
        // Arrange
        var originalRsvp = new Rsvp 
        {
            Name = "Original Name",
            NumberOfGuestOvernight = 5,
            Language = Language.de,
        };
        var createResult = await _serviceUnderTest.CreateAsync(originalRsvp);

        var updatedRsvp = new Rsvp
        {
            Id = createResult.ValueSuccess!.Id,
            Name = "Updated Name",
            NumberOfGuestOvernight = 6,
            NumberOfMeatMenus = 2,
            NumberOfFishMenus = 2,
            NumberOfVegetarianMenus = 1,
            AdditionalInformation = "Updated info",
            Language = Language.de,
            LastUpdated = TimeProvider.GetUtcNow().DateTime,
        };

        // Act
        var result = await _serviceUnderTest.UpdateAsync(updatedRsvp);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ValueSuccess.Should().BeEquivalentTo(updatedRsvp);

        var dbRsvpResponse = await _serviceUnderTest.ReadAsync(Guid.Parse(result.ValueSuccess.Id));
        dbRsvpResponse.IsSuccess.Should().BeTrue();
        dbRsvpResponse.ValueSuccess.Should().BeEquivalentTo(updatedRsvp);
    }
    
    [Fact]
    public async Task UpdateAsync_WithSameEntity_ShouldUpdateRsvp()
    {
        // Arrange
        var originalRsvp = new Rsvp 
        {
            Name = "Original Name",
            NumberOfGuestOvernight = 5,
            Language = Language.de,
        };
        var createResult = await _serviceUnderTest.CreateAsync(originalRsvp);

        var updatedRsvp = new Rsvp
        {
            Id = createResult.ValueSuccess!.Id,
            Name = "Original Name",
            NumberOfGuestOvernight = 5,
            Language = Language.de,
            LastUpdated = TimeProvider.GetUtcNow().DateTime,
        };

        // Act
        var result = await _serviceUnderTest.UpdateAsync(updatedRsvp);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ValueSuccess.Should().BeEquivalentTo(updatedRsvp);

        var dbRsvpResponse = await _serviceUnderTest.ReadAsync(Guid.Parse(result.ValueSuccess.Id));
        dbRsvpResponse.IsSuccess.Should().BeTrue();
        dbRsvpResponse.ValueSuccess.Should().BeEquivalentTo(updatedRsvp);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        var originalRsvp = new Rsvp 
        {
            Name = "Original Name",
            NumberOfGuestOvernight = 5,
            Language = Language.en,
        };
        var createResult = await _serviceUnderTest.CreateAsync(originalRsvp);

        var updatedRsvp = new Rsvp
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Updated Name",
            NumberOfGuestOvernight = 6,
            NumberOfMeatMenus = 2,
            NumberOfFishMenus = 2,
            NumberOfVegetarianMenus = 1,
            AdditionalInformation = "Updated info",
            Language = Language.en,
            LastUpdated = TimeProvider.GetUtcNow().DateTime,
        };

        // Act
        var result = await _serviceUnderTest.UpdateAsync(updatedRsvp);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValueFail.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}