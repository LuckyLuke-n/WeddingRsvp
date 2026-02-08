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
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Services;
using WeddingRsvp.Api.Services.Generics;

namespace WeddingRsvp.Integration;

public class SettingsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<ISettingsService> _serviceMock = new();
    private const string ApiKeyHeader = "X-Api-Key";
    private const string ApiKey = "api-key";

    public SettingsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Add a dummy connection string to satisfy startup validation
            builder.UseSetting("ConnectionStrings:weddingrsvp-mongo", "mongodb://localhost:27017");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ISettingsService>();
                services.AddSingleton(_serviceMock.Object);

                services.Configure<ApiConfiguration>(opts =>
                {
                    opts.ApiKey = ApiKey;
                });
            });
        });
    }

    [Fact]
    public async Task Get_WithoutApiKey_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        _serviceMock.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResponse<Settings>.CreateSuccess(new Settings()));

        var response = await client.GetAsync("/api/settings");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_WithApiKey_ReturnsOk()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);

        _serviceMock.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResponse<Settings>.CreateSuccess(new Settings()));

        var response = await client.GetAsync("/api/settings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Put_WithApiKey_ReturnsOk()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);

        _serviceMock.Setup(x => x.UpsertAsync(It.IsAny<Settings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResponse<Settings>.CreateSuccess(new Settings()));

        var dto = new PutSettingsDto
        {
            EnableEmailNotifications = false,
            EmailRecipients = ["updated@example.com"],
            RespondUntil = new DateTime(2031, 2, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        var response = await client.PutAsJsonAsync("/api/settings", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    
    [Fact]
    public async Task Put_WithOutApiKey_ReturnsOk()
    {
        var client = _factory.CreateClient();

        _serviceMock.Setup(x => x.UpsertAsync(It.IsAny<Settings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResponse<Settings>.CreateSuccess(new Settings()));

        var dto = new PutSettingsDto
        {
            EnableEmailNotifications = false,
            EmailRecipients = ["updated@example.com"],
            RespondUntil = new DateTime(2031, 2, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        _serviceMock.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResponse<Settings>.CreateSuccess(new Settings()));

        var response = await client.PutAsJsonAsync("/api/settings", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}