using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using WeddingRsvp.Abstractions.Models.Information;
using WeddingRsvp.Api.Configurations;
using WeddingRsvp.Api.Repository;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;

namespace WeddingRsvp.Integration;

public class InformationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IInformationRepository> _repositoryMock = new();
    private const string AdminHeaderName = "X-Auth-Admin";
    private const string ApiKeyHeader = "X-Api-Key";
    private const string AdminSecret = "secret-key";
    private const string ApiKey = "api-key";

    public InformationControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:weddingrsvp-mongo", "mongodb://localhost:27017");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IInformationRepository>();
                services.AddSingleton(_repositoryMock.Object);

                services.Configure<ApiConfiguration>(opts =>
                {
                    opts.AdminIdentifier = AdminSecret;
                    opts.ApiKey = ApiKey;
                });
            });
        });
    }

    [Fact]
    public async Task GetAll_WithoutAdminHeader_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);

        var infoList = new List<Information> { new() { Id = Guid.NewGuid().ToString(), Language = "en" } };
        _repositoryMock.Setup(x => x.ReadAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<IEnumerable<Information>, RepositoryFailResponse>.CreateSuccess(infoList));

        // Act
        var response = await client.GetAsync("/api/information");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetByLanguage_WhenLanguageExists_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        var language = "es";

        var info = new Information
        {
            Id = Guid.NewGuid().ToString(),
            Language = language,
            InvitationText = "¡Hola!"
        };

        _repositoryMock.Setup(x => x.ReadByLanguageAsync(language, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Information, RepositoryFailResponse>.CreateSuccess(info));

        // Act
        var response = await client.GetAsync($"/api/information/language/{language}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultDto = await response.Content.ReadFromJsonAsync<GetInformationDto>();
        resultDto.Should().NotBeNull();
        resultDto!.Language.Should().Be(language);
    }

    [Fact]
    public async Task GetByLanguage_WhenLanguageDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        var language = "it";

        _repositoryMock.Setup(x => x.ReadByLanguageAsync(language, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Information, RepositoryFailResponse>.CreateFail(new RepositoryFailResponse
            {
                StatusCode = HttpStatusCode.NotFound
            }));

        // Act
        var response = await client.GetAsync($"/api/information/language/{language}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_WithoutApiKeyHeader_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/information");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithValidDataAndAdminHeader_ReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        client.DefaultRequestHeaders.Add(AdminHeaderName, AdminSecret);

        var dto = new PostInformationDto
        {
            Language = "en",
            InvitationText = "Welcome",
            Itinerary = new Dictionary<string, string> { { "12:00", "Ceremony" } },
            Faqs = new Dictionary<string, string> { { "Parking?", "Yes" } }
        };

        var createdInfo = new Information { Id = Guid.NewGuid().ToString(), Language = "en" };
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<Information>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Information, RepositoryFailResponse>.CreateSuccess(createdInfo));

        // Act
        var response = await client.PostAsJsonAsync("/api/information", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Delete_WithValidAdminHeader_ReturnsNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        client.DefaultRequestHeaders.Add(AdminHeaderName, AdminSecret);
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<RepositoryFailResponse>.CreateSuccess());

        // Act
        var response = await client.DeleteAsync($"/api/information/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WithoutValidAdminHeader_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<RepositoryFailResponse>.CreateSuccess());

        // Act
        var response = await client.DeleteAsync($"/api/information/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_WithValidAdminHeader_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        client.DefaultRequestHeaders.Add(AdminHeaderName, AdminSecret);
        var id = Guid.NewGuid();

        var dto = new PutInformationDto
        {
            Language = "en",
            InvitationText = "Updated Text",
            Itinerary = new Dictionary<string, string> { { "13:00", "Lunch" } },
            Faqs = new Dictionary<string, string> { { "Dresscode?", "Casual" } }
        };

        var updatedInfo = new Information { Id = id.ToString(), Language = "en" };
        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Information>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Information, RepositoryFailResponse>.CreateSuccess(updatedInfo));
        _repositoryMock.Setup(x => x.ReadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Information, RepositoryFailResponse>.CreateSuccess(updatedInfo));

        // Act
        var response = await client.PutAsJsonAsync($"/api/information/{id}", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_WithInvalidAdminHeader_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        client.DefaultRequestHeaders.Add(AdminHeaderName, "wrong-secret");
        var id = Guid.NewGuid();

        var dto = new PutInformationDto
        {
            Language = "en",
            InvitationText = "Updated Text",
            Itinerary = new Dictionary<string, string> { { "13:00", "Lunch" } },
            Faqs = new Dictionary<string, string> { { "Dresscode?", "Casual" } }
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/information/{id}", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}