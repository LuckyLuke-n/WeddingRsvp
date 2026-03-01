using System.Net;
using System.Net.Http.Json;
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

public class InformationsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IInformationRepository> _repositoryMock = new();
    private const string AdminHeaderName = "X-Auth-Admin";
    private const string ApiKeyHeader = "X-Api-Key";
    private const string AdminSecret = "secret-key";
    private const string ApiKey = "api-key";

    public InformationsControllerTests(WebApplicationFactory<Program> factory)
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
        client.DefaultRequestHeaders.Add(AdminHeaderName, AdminSecret);

        var infoList = new List<Information> { new() { Id = Guid.NewGuid().ToString(), Language = "en" } };
        _repositoryMock.Setup(x => x.ReadAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<IEnumerable<Information>, RepositoryFailResponse>.CreateSuccess(infoList));

        // Act
        var response = await client.GetAsync("/api/informations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAll_WitAdminHeader_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);

        var infoList = new List<Information> { new() { Id = Guid.NewGuid().ToString(), Language = "en" } };
        _repositoryMock.Setup(x => x.ReadAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<IEnumerable<Information>, RepositoryFailResponse>.CreateSuccess(infoList));

        // Act
        var response = await client.GetAsync("/api/informations");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
        var response = await client.GetAsync($"/api/informations/language/{language}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var resultDto = await response.Content.ReadFromJsonAsync<GetInformationDto>();
        Assert.NotNull(resultDto);
        Assert.Equal(language, resultDto!.Language);
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
        var response = await client.GetAsync($"/api/informations/language/{language}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithoutApiKeyHeader_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/informations");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
        var response = await client.PostAsJsonAsync("/api/informations", dto);

        // Assert
        var result = await response.Content.ReadFromJsonAsync<GetInformationDto>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equivalent(createdInfo.ToDto(), result);
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
        var response = await client.DeleteAsync($"/api/informations/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
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
        var response = await client.DeleteAsync($"/api/informations/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
        var response = await client.PutAsJsonAsync($"/api/informations/{id}", dto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetInformationDto>();
        Assert.NotNull(result);
        Assert.Equivalent(updatedInfo.ToDto(), result);
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
        var response = await client.PutAsJsonAsync($"/api/informations/{id}", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
