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
using WeddingRsvp.Integration.Fixtures;
using WeddingRsvp.Api.Repository.Entities;
using Language = WeddingRsvp.Api.Repository.Entities.Language;
using Reply = WeddingRsvp.Abstractions.Models.Reply;

namespace WeddingRsvp.Integration;

public class RsvpControllerTests : IClassFixture<WebApplicationFactory<Program>>, IClassFixture<TimeProviderFixture>
{
    private TimeProvider TimeProvider { get; }
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IRsvpRepository> _repositoryMock = new();
    private const string AdminHeaderName = "X-Auth-Admin";
    private const string ApiKeyHeader = "X-Api-Key";
    private const string AdminSecret = "secret-key";
    private const string ApiKey = "api-key";

    public RsvpControllerTests(WebApplicationFactory<Program> factory, TimeProviderFixture timeFixture )
    {
        timeFixture.ProviderMock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2022, 1, 1, 12, 0, 0, TimeSpan.Zero));
        TimeProvider = timeFixture.ProviderMock.Object;

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
            new() { Id = Guid.NewGuid().ToString(), Name = "Alice", NumberOfGuestOvernight = 1 },
            new() { Id = Guid.NewGuid().ToString(), Name = "Bob", NumberOfGuestOvernight = 2 }
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
            new() { Id = Guid.NewGuid().ToString(), Name = "Alice", NumberOfGuestOvernight = 1 },
            new() { Id = Guid.NewGuid().ToString(), Name = "Bob", NumberOfGuestOvernight = 2 }
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
        var rsvp = new Rsvp { Id = id.ToString(), Name = "Charlie", Language = Language.en, LastUpdated = TimeProvider.GetUtcNow().UtcDateTime };
        
        var dto = new GetRsvpDto
        {
             Id = id.ToString(),
             Name = "Charlie",
             Language = Abstractions.Models.Language.en,
             LastUpdated = TimeProvider.GetUtcNow().UtcDateTime,
        };

        _repositoryMock.Setup(x => x.ReadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateSuccess(rsvp));

        // Act
        var response = await client.GetAsync($"/api/rsvps/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Rsvp>(); // The controller returns DTO, assuming JSON compatibility
        result.Should().NotBeNull();
        result!.ToDto().Should().BeEquivalentTo(dto);
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
        var dto = new PostRsvpDto { Name = "Dave", Salutation = "Dear Dave", Language = Abstractions.Models.Language.en} ;
        
        // Ensure the return object is fully initialized
        var createdRsvp = new Rsvp 
        { 
            Id = Guid.NewGuid().ToString(), // Ensure ID is set if needed by response handling
            Name = "Dave", 
            Salutation = "Dear Dave",
            Language = Language.en
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
            Salutation = "Dear Eve",
            Language = Language.en,
            IsPlural = false,
        };

        // Incoming update (changing only AdditionalInformation, which is not sensitive)
        var updateDto = new PutRsvpDto 
        {
            Name = "Eve", 
            Salutation = "Dear Eve",
            IsPlural = false,
            Attending = Reply.Yes,
            BringPartner = Reply.No,
            Language = Abstractions.Models.Language.en,
            NumberOfChildren = 2,
            NumberOfMeatMenus = 1,
            NumberOfVegetarianMenus = 1,
            NumberOfFishMenus = 0,
            AdditionalInformation = "New Info"
        };

        var updatedRsvp = new Rsvp
        {
            Id = id.ToString(), 
            Name = "Eve", 
            Salutation = "Dear Eve",
            Attending = (Api.Repository.Entities.Reply)Reply.Yes,
            BringPartner = (Api.Repository.Entities.Reply)Reply.No,
            NumberOfGuestOvernight = 2,
            NumberOfMeatMenus = 1,
            NumberOfFishMenus = 0,
            NumberOfVegetarianMenus = 1,
            AdditionalInformation = "New Info",
            Language = Language.en,
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
    public async Task Update_NonCriticalData_WithExistingData_DoesNotRequireAuth_ReturnsOk()
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
            Salutation = "Dear Eve",
            Language = Language.en,
            IsPlural = false,
            NumberOfGuestOvernight = 2,
            NumberOfMeatMenus = 1,
            NumberOfFishMenus = 1,
            NumberOfVegetarianMenus = 0,
            AdditionalInformation = "Old Info",
        };

        // Incoming update (changing only AdditionalInformation, which is not sensitive)
        var updateDto = new PutRsvpDto 
        {
            Name = "Eve",
            Salutation = "Dear Eve",
            Language = (Abstractions.Models.Language)Language.en,
            IsPlural = false,
            NumberOfChildren = 2,
            NumberOfMeatMenus = 1,
            NumberOfFishMenus = 1,
            NumberOfVegetarianMenus = 0,
            AdditionalInformation = "Old Info",
        };

        var updatedRsvp = new Rsvp
        {
            Id = id.ToString(), 
            Name = "Eve", 
            Salutation = "Dear Eve",
            Language = Language.en,
            IsPlural = false,
            NumberOfGuestOvernight = 2,
            NumberOfMeatMenus = 1,
            NumberOfFishMenus = 1,
            NumberOfVegetarianMenus = 0,
            AdditionalInformation = "Old Info"
        };

        _repositoryMock.Setup(x => x.ReadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateSuccess(existingRsvp));

        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Rsvp>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateSuccess(updatedRsvp));

        // Act - NO HEADER provided
        var response = await client.PutAsJsonAsync($"/api/rsvps/{id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Rsvp>();
        result!.ToDto().Should().BeEquivalentTo(updateDto);
    }

    [Theory]
    [InlineData("eb34fe42-920b-4b1e-a9c3-ccb2f8f6afbf", 2, 1, 1, 0, "New Info", Language.de)]
    [InlineData("b695dcb1-98a7-495f-a591-83f7ce1cd9a5", 4, 2, 2, 0, "All attending", Language.en)]
    [InlineData("e67b9d97-8213-4463-a2ba-ae813e64e76f", 1, 0, 0, 0, "Not attending", Language.de)]
    public async Task Update_NonCriticalData_WithoutAuth_ReturnsForbidden(
        string id,
        int children, 
        int meat, 
        int vegetarian, 
        int fish, 
        string info,
        Language language)
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        
        var existingRsvp = new Rsvp
            {
                Id = id,
                Name = "Frank",
                Salutation = "Dear Frank",
                IsPlural = true,
                Language = Language.en,
            };
        
        // Trying to change Name (Critical) or Language (Critical)
        var updateDto = new PutRsvpDto 
        { 
            Name = "Frank Updated", 
            Salutation = "Dear Frank",
            IsPlural = true,
            NumberOfChildren = children,
            NumberOfMeatMenus = meat,
            NumberOfVegetarianMenus = vegetarian,
            NumberOfFishMenus = fish,
            AdditionalInformation = info,
            Language = (Abstractions.Models.Language)language,
        };

        _repositoryMock.Setup(x => x.ReadAsync(Guid.Parse(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateSuccess(existingRsvp));

        // Act
        var response = await client.PutAsJsonAsync($"/api/rsvps/{id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Rsvp>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Update_CriticalData_WithValidAdminHeader_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        client.DefaultRequestHeaders.Add(AdminHeaderName, AdminSecret);
        var id = Guid.NewGuid();

        var existingRsvp = new Rsvp
        {
            Id = id.ToString(),
            Name = "Frank",
            Salutation = "Dear Frank",
            IsPlural = false,
            Language = Language.en,
        };

        var updateDto = new PutRsvpDto
        {
            Name = "Frank Updated",
            Salutation = "Dear Frank",
            IsPlural = true,
            Language = Abstractions.Models.Language.de,
            NumberOfChildren = 2,
            NumberOfMeatMenus = 2,
            AdditionalInformation = "Updating critical fields"
        };

        var updatedRsvp = new Rsvp
        {
            Id = id.ToString(),
            Name = "Frank Updated",
            Salutation = "Dear Frank",
            IsPlural = true,
            Language = Language.de,
            NumberOfGuestOvernight = 2,
            NumberOfMeatMenus = 2,
            AdditionalInformation = "Updating critical fields"
        };

        _repositoryMock.Setup(x => x.ReadAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateSuccess(existingRsvp));

        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Rsvp>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RepositoryResponse<Rsvp, RepositoryFailResponse>.CreateSuccess(updatedRsvp));

        // Act
        var response = await client.PutAsJsonAsync($"/api/rsvps/{id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetRsvpDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Frank Updated");
        _repositoryMock.Verify(x => x.UpdateAsync(It.Is<Rsvp>(r => r.Name == "Frank Updated"), It.IsAny<CancellationToken>()), Times.Once);
    }
}