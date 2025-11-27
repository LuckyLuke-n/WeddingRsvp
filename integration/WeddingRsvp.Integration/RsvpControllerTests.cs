using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using WeddingRsvp.Abstractions.Models;
using WeddingRsvp.Api.Configurations;
using WeddingRsvp.Api.Repository;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;
using GuestType = WeddingRsvp.Abstractions.Models.GuestType;

namespace WeddingRsvp.Integration;

public class RsvpControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IRsvpRepository> _repositoryMock = new();
    private const string AdminHeaderName = "X-Auth-Admin";
    private const string ApiKeyHeader = "X-Api-Key";
    private const string AdminSecret = "secret-key";
    private const string ApiKey = "api-key";

    public RsvpControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Add a dummy connection string to satisfy startup validation
            builder.UseSetting("ConnectionStrings:weddingrsvp-mongo", "mongodb://localhost:27017");

            builder.ConfigureTestServices(services =>
            {
                // Replace the real repository with the mock
                services.RemoveAll<IRsvpRepository>();
                services.AddSingleton(_repositoryMock.Object);

                // Ensure the configuration matches what we expect for auth tests
                services.Configure<ApiConfiguration>(opts =>
                {
                    opts.AdminIdentifier = AdminSecret;
                    opts.ApiKey = ApiKey;
                });
            });
        });
    }

    [Fact]
    public async Task GetAll_WithInvalidApiKey_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var rsvps = new List<Rsvp>
        {
            new() { Id = Guid.NewGuid().ToString(), Name = "Alice", NumberOfGuests = 1 },
            new() { Id = Guid.NewGuid().ToString(), Name = "Bob", NumberOfGuests = 2 }
        };

        _repositoryMock.Setup(x => x.ReadAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<IEnumerable<Rsvp>, RepositoryFailResponse>.CreateSuccess(rsvps));

        client.DefaultRequestHeaders.Add(AdminHeaderName, AdminSecret);

        // Act
        var response = await client.GetAsync("/api/rsvps");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task GetAll_WithValidAdminHeader_ReturnsOkAndList()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        var rsvps = new List<Rsvp>
        {
            new() { Id = Guid.NewGuid().ToString(), Name = "Alice", NumberOfGuests = 1 },
            new() { Id = Guid.NewGuid().ToString(), Name = "Bob", NumberOfGuests = 2 }
        };

        _repositoryMock.Setup(x => x.ReadAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<IEnumerable<Rsvp>, RepositoryFailResponse>.CreateSuccess(rsvps));

        client.DefaultRequestHeaders.Add(AdminHeaderName, AdminSecret);

        // Act
        var response = await client.GetAsync("/api/rsvps");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<Rsvp>>();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithoutHeader_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);

        // Act
        var response = await client.GetAsync("/api/rsvps");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Get_ById_ReturnsOk_WhenFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        var id = Guid.NewGuid();
        var rsvp = new Rsvp { Id = id.ToString(), Name = "Charlie" };

        _repositoryMock.Setup(x => x.ReadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateSuccess(rsvp));

        // Act
        var response = await client.GetAsync($"/api/rsvps/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Rsvp>(); // The controller returns DTO, assuming JSON compatibility
        result.Should().NotBeNull();
        result!.Name.Should().Be("Charlie");
    }

    [Fact]
    public async Task Get_ById_ReturnsNotFound_WhenRepoReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.ReadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateFail(
                new RepositoryFailResponse(HttpStatusCode.NotFound, "Not Found")));

        // Act
        var response = await client.GetAsync($"/api/rsvps/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidDataAndAuth_ReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        var dto = new PostRsvpDto { Name = "Dave", NumberOfGuests = 1, Type = GuestType.Friends };
        
        // Ensure the return object is fully initialized
        var createdRsvp = new Rsvp 
        { 
            Id = Guid.NewGuid().ToString(), // Ensure ID is set if needed by response handling
            Name = "Dave", 
            NumberOfGuests = 1, 
            Type = WeddingRsvp.Api.Repository.Entities.GuestType.Friends 
        };

        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<Rsvp>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateSuccess(createdRsvp));

        client.DefaultRequestHeaders.Add(AdminHeaderName, AdminSecret);

        // Act
        var response = await client.PostAsJsonAsync("/api/rsvps", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Delete_WithAuth_ReturnsNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        var id = Guid.NewGuid();

        _repositoryMock.Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<RepositoryFailResponse>.CreateSuccess());

        client.DefaultRequestHeaders.Add(AdminHeaderName, AdminSecret);

        // Act
        var response = await client.DeleteAsync($"/api/rsvps/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_NonCriticalData_DoesNotRequireAuth_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        var id = Guid.NewGuid();
        
        // Existing entity
        var existingRsvp = new Rsvp 
        { 
            Id = id.ToString(), 
            Name = "Eve", 
            Type = WeddingRsvp.Api.Repository.Entities.GuestType.Friends, 
            NumberOfGuests = 2,
            AdditionalInformation = "Old Info"
        };

        // Incoming update (changing only AdditionalInformation, which is not sensitive)
        var updateDto = new PutRsvpDto 
        {
            Name = "Eve", 
            Type = GuestType.Friends, 
            NumberOfGuests = 2,
            NumberOfGuestsAttending = 2,
            NumberOfNormalMeals = 1,
            NumberOfVegetarianMeals = 1,
            NumberOfVeganMeals = 0,
            AdditionalInformation = "New Info"
        };

        var updatedRsvp = new Rsvp
        {
            Id = id.ToString(), 
            Name = "Eve", 
            Type = WeddingRsvp.Api.Repository.Entities.GuestType.Friends, 
            NumberOfGuests = 2,
            NumberOfGuestsAttending = 2,
            NumberOfNormalMeals = 1,
            NumberOfVegetarianMeals = 1,
            NumberOfVeganMeals = 0,
            AdditionalInformation = "New Info"
        };

        _repositoryMock.Setup(x => x.ReadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateSuccess(existingRsvp));

        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Rsvp>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateSuccess(updatedRsvp));

        // Act - NO HEADER provided
        var response = await client.PutAsJsonAsync($"/api/rsvps/{id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _repositoryMock.Verify(x => x.UpdateAsync(It.Is<Rsvp>(r => r.AdditionalInformation == "New Info"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_CriticalData_WithoutAuth_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        var id = Guid.NewGuid();
        
        var existingRsvp = new Rsvp
            {
                Id = id.ToString(),
                Name = "Frank",
                NumberOfGuests = 2,
                Type = WeddingRsvp.Api.Repository.Entities.GuestType.Friends
            };
        
        // Trying to change NumberOfGuests
        var updateDto = new PutRsvpDto 
        { 
            Name = "Frank", 
            Type = GuestType.Friends, 
            NumberOfGuests = 9,
            NumberOfGuestsAttending = 9,
            NumberOfNormalMeals = 7,
            NumberOfVegetarianMeals = 1,
            NumberOfVeganMeals = 1,
            AdditionalInformation = "New Info"
        };

        _repositoryMock.Setup(x => x.ReadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateSuccess(existingRsvp));

        // Act
        var response = await client.PutAsJsonAsync($"/api/rsvps/{id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Rsvp>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}