using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using WeddingRsvp.Abstractions.Models.Settings;
using WeddingRsvp.Api.Configurations;
using WeddingRsvp.Api.Repository;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;

namespace WeddingRsvp.Integration;

public class SettingsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<ISettingsRepository> _repositoryMock = new();
    private const string ApiKeyHeader = "X-Api-Key";
    private const string ApiKey = "api-key";

    public SettingsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:weddingrsvp-mongo", "mongodb://localhost:27017");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ISettingsRepository>();
                services.AddSingleton(_repositoryMock.Object);

                services.Configure<ApiConfiguration>(opts =>
                {
                    opts.ApiKey = ApiKey;
                });
            });
        });
    }

    [Fact]
    public async Task Get_WithValidApiKey_ReturnsOkAndList()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);

        var id = Guid.NewGuid();
        var settings = new Settings
        {
            Id = id.ToString(),
            EnableEmailNotifications = true,
            EmailRecipients = ["test@example.com"],
            RespondUntil = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        _repositoryMock.Setup(x => x.ReadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Settings, RepositoryFailResponse>.CreateSuccess(settings));

        // Act
        var response = await client.GetAsync($"/api/settings/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<GetSettingsDto>>();
        result.Should().NotBeNull();
        result!.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(settings.ToDto());
    }

    [Fact]
    public async Task Get_WhenRepositoryFails_ReturnsInternalServerError()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);

        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.ReadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Settings, RepositoryFailResponse>.CreateFail(
                new RepositoryFailResponse(HttpStatusCode.InternalServerError, "error")));

        // Act
        var response = await client.GetAsync($"/api/settings/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Get_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/settings/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_WithValidApiKey_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);

        var id = Guid.NewGuid();
        var dto = new PutSettingsDto
        {
            EnableEmailNotifications = false,
            EmailRecipients = ["updated@example.com"],
            RespondUntil = new DateTime(2031, 2, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Settings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Settings, RepositoryFailResponse>.CreateSuccess(new Settings { Id = id.ToString() }));

        // Act
        var response = await client.PutAsJsonAsync($"/api/settings/{id}", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _repositoryMock.Verify(x =>
            x.UpdateAsync(It.Is<Settings>(s => s.Id == id.ToString()), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenRepositoryFails_ReturnsInternalServerError()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);

        var id = Guid.NewGuid();
        var dto = new PutSettingsDto
        {
            EnableEmailNotifications = false,
            EmailRecipients = ["updated@example.com"],
            RespondUntil = new DateTime(2031, 2, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Settings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Settings, RepositoryFailResponse>.CreateFail(
                new RepositoryFailResponse(HttpStatusCode.InternalServerError, "error")));

        // Act
        var response = await client.PutAsJsonAsync($"/api/settings/{id}", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Update_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = Guid.NewGuid();
        var dto = new PutSettingsDto
        {
            EnableEmailNotifications = false,
            EmailRecipients = ["updated@example.com"],
            RespondUntil = new DateTime(2031, 2, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/settings/{id}", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}