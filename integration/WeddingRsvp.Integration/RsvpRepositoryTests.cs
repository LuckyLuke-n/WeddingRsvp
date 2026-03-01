using System.Net;
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
    private readonly MongoDbContainer _mongoDbContainer = new MongoDbBuilder("mongo:8").Build();
    private IRsvpRepository _serviceUnderTest = null!;
    private IMongoClient _mongoClient = null!;
    private TimeProvider? TimeProvider { get; set; }

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
            Name = "John Doe",
            NumberOfGuestsOvernight = 2,
            NumberOfMeatMenus = 1,
            NumberOfBrunchGuests = 1,
            NumberOfVegetarianMenus = 0,
            AdditionalInformation = ""
        };

        // Act
        var result = await _serviceUnderTest.CreateAsync(rsvp);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(rsvp, result.ValueSuccess);

        var dbRsvpResponse = await _serviceUnderTest.ReadAsync(Guid.Parse(rsvp.Id));
        Assert.True(dbRsvpResponse.IsSuccess);
        Assert.Equivalent(rsvp, dbRsvpResponse.ValueSuccess);
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnRsvp_WhenExists()
    {
        // Arrange
        var rsvp = new Rsvp
        {
            Name = "Jane Doe",
            NumberOfGuestsOvernight = 1,
        };
        await _serviceUnderTest.CreateAsync(rsvp);

        // Act
        var result = await _serviceUnderTest.ReadAsync(Guid.Parse(rsvp.Id));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(rsvp, result.ValueSuccess);
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
        Assert.True(result.IsSuccess);
        Assert.Contains(result.ValueSuccess!, rsvp => rsvp.Name == rsvp1.Name);
        Assert.Contains(result.ValueSuccess!, rsvp => rsvp.Name == rsvp2.Name);
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
        Assert.True(result.IsSuccess);

        var readResult = await _serviceUnderTest.ReadAsync(Guid.Parse(rsvp.Id));
        Assert.False(readResult.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, readResult.ValueFail.StatusCode);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldUpdateRsvp()
    {
        // Arrange
        var originalRsvp = new Rsvp 
        {
            Name = "Original Name",
            NumberOfGuestsOvernight = 5,
        };
        var createResult = await _serviceUnderTest.CreateAsync(originalRsvp);

        var updatedRsvp = new Rsvp
        {
            Id = createResult.ValueSuccess!.Id,
            Name = "Updated Name",
            NumberOfGuestsOvernight = 6,
            NumberOfMeatMenus = 2,
            NumberOfBrunchGuests = 2,
            NumberOfVegetarianMenus = 1,
            AdditionalInformation = "Updated info",
            LastUpdated = TimeProvider!.GetUtcNow().DateTime,
        };

        // Act
        var result = await _serviceUnderTest.UpdateAsync(updatedRsvp);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(updatedRsvp, result.ValueSuccess);

        var dbRsvpResponse = await _serviceUnderTest.ReadAsync(Guid.Parse(result.ValueSuccess.Id));
        Assert.True(dbRsvpResponse.IsSuccess);
        Assert.Equivalent(updatedRsvp, dbRsvpResponse.ValueSuccess);
    }
    
    [Fact]
    public async Task UpdateAsync_WithSameEntity_ShouldUpdateRsvp()
    {
        // Arrange
        var originalRsvp = new Rsvp 
        {
            Name = "Original Name",
            NumberOfGuestsOvernight = 5,
        };
        var createResult = await _serviceUnderTest.CreateAsync(originalRsvp);

        var updatedRsvp = new Rsvp
        {
            Id = createResult.ValueSuccess!.Id,
            Name = "Original Name",
            NumberOfGuestsOvernight = 5,
            LastUpdated = TimeProvider!.GetUtcNow().DateTime,
        };

        // Act
        var result = await _serviceUnderTest.UpdateAsync(updatedRsvp);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(updatedRsvp, result.ValueSuccess);

        var dbRsvpResponse = await _serviceUnderTest.ReadAsync(Guid.Parse(result.ValueSuccess!.Id));
        Assert.True(dbRsvpResponse.IsSuccess);
        Assert.Equivalent(updatedRsvp, dbRsvpResponse.ValueSuccess);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        var originalRsvp = new Rsvp 
        {
            Name = "Original Name",
            NumberOfGuestsOvernight = 5,
        };
        await _serviceUnderTest.CreateAsync(originalRsvp);

        var updatedRsvp = new Rsvp
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Updated Name",
            NumberOfGuestsOvernight = 6,
            NumberOfMeatMenus = 2,
            NumberOfBrunchGuests = 2,
            NumberOfVegetarianMenus = 1,
            AdditionalInformation = "Updated info",
            LastUpdated = TimeProvider!.GetUtcNow().DateTime,
        };

        // Act
        var result = await _serviceUnderTest.UpdateAsync(updatedRsvp);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.ValueFail.StatusCode);
    }
}
