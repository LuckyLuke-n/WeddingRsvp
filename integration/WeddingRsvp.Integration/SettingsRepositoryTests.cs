using System.Net;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Testcontainers.MongoDb;
using WeddingRsvp.Api.Repository;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;

namespace WeddingRsvp.Integration;

public class SettingsRepositoryTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoDbContainer = new MongoDbBuilder("mongo:8").Build();
    private ISettingsRepository _serviceUnderTest = null!;
    private IMongoClient _mongoClient = null!;

    public async Task InitializeAsync()
    {
        await _mongoDbContainer.StartAsync();
        var connectionString = _mongoDbContainer.GetConnectionString();
        _mongoClient = new MongoClient(connectionString);

        var loggerMock = new Mock<ILogger<MongoDbRepository<Settings>>>();
        _serviceUnderTest = new SettingsRepository(_mongoClient, loggerMock.Object);
    }

    public async Task DisposeAsync()
    {
        await _mongoDbContainer.StopAsync();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateSettings()
    {
        // Arrange
        var id = Guid.NewGuid();
        var settings = new Settings
        {
            Id = id.ToString(),
            EnableEmailNotifications = true,
            EmailRecipients = ["create@example.com"],
            RespondUntil = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc),
        };

        // Act
        var result = await _serviceUnderTest.CreateAsync(settings);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(settings, result.ValueSuccess);

        var dbResponse = await _serviceUnderTest.ReadAsync(id);
        Assert.True(dbResponse.IsSuccess);
        Assert.Equivalent(settings, dbResponse.ValueSuccess);
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnSettings_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var settings = new Settings
        {
            Id = id.ToString(),
            EnableEmailNotifications = false,
            EmailRecipients = ["read@example.com"],
            RespondUntil = new DateTime(2031, 2, 2, 12, 0, 0, DateTimeKind.Utc),
        };
        await _serviceUnderTest.CreateAsync(settings);

        // Act
        var result = await _serviceUnderTest.ReadAsync(id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(settings, result.ValueSuccess);
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
    public async Task UpdateAsync_ShouldUpdateSettings()
    {
        // Arrange
        var seed = new Settings
        {
            Id = Guid.NewGuid().ToString(),
            EnableEmailNotifications = true,
            EmailRecipients = ["original@example.com"],
            RespondUntil = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc),
        };
        await _serviceUnderTest.UpdateAsync(seed);

        var updated = new Settings
        {
            Id = seed.Id,
            EnableEmailNotifications = false,
            EmailRecipients = ["updated@example.com", "other@example.com"],
            RespondUntil = new DateTime(2031, 2, 2, 12, 0, 0, DateTimeKind.Utc),
        };

        // Act
        var result = await _serviceUnderTest.UpdateAsync(updated);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(updated, result.ValueSuccess);

        var dbResponse = await _serviceUnderTest.ReadAsync(Guid.Parse(updated.Id));
        Assert.True(dbResponse.IsSuccess);
        Assert.Equivalent(updated, dbResponse.ValueSuccess);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpsertSettings_WhenNotExists()
    {
        // Arrange
        var upserted = new Settings
        {
            Id = Guid.NewGuid().ToString(),
            EnableEmailNotifications = false,
            EmailRecipients = ["new@example.com"],
            RespondUntil = new DateTime(2032, 3, 3, 12, 0, 0, DateTimeKind.Utc),
        };

        // Act
        var result = await _serviceUnderTest.UpdateAsync(upserted);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(upserted, result.ValueSuccess);

        var dbResponse = await _serviceUnderTest.ReadAsync(Guid.Parse(upserted.Id));
        Assert.True(dbResponse.IsSuccess);
        Assert.Equivalent(upserted, dbResponse.ValueSuccess);
    }
}
