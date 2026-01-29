using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using WeddingRsvp.Abstractions.Models.Notifications;
using WeddingRsvp.Api.Configurations;
using WeddingRsvp.Api.Services;
using WeddingRsvp.Api.Services.Generics;

namespace WeddingRsvp.Integration;

public class NotificationsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private const string ApiKeyHeader = "X-Api-Key";
    private const string ApiKey = "api-key";

    public NotificationsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:weddingrsvp-mongo", "mongodb://localhost:27017");

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IEmailService>();
                services.AddSingleton(_emailServiceMock.Object);

                services.Configure<ApiConfiguration>(opts =>
                {
                    opts.ApiKey = ApiKey;
                });
            });
        });
    }

    [Fact]
    public async Task SendEmailNotification_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var dto = new PostEmailDto
        {
            Name = "John Doe",
            Attending = "Yes",
            BringPartner = "No",
            NumberOfGuestsOvernight = 2,
            NumberOfMeatMenus = 1,
            NumberOfVegetarianMenus = 1,
            NumberOfBrunchGuests = 2,
            AdditionalInformation = "Looking forward to it!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/notifications", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        _emailServiceMock.Verify(x => x.SendRsvpConfirmationAsync(It.IsAny<EmailTemplate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendEmailNotification_WithInvalidApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, "invalid-key");
        
        var dto = new PostEmailDto
        {
            Name = "John Doe",
            Attending = "Yes",
            BringPartner = "No",
            NumberOfGuestsOvernight = 2,
            NumberOfMeatMenus = 1,
            NumberOfVegetarianMenus = 1,
            NumberOfBrunchGuests = 2,
            AdditionalInformation = "Looking forward to it!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/notifications", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        _emailServiceMock.Verify(x => x.SendRsvpConfirmationAsync(It.IsAny<EmailTemplate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendEmailNotification_WithValidApiKey_WhenEmailServiceSucceeds_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        
        var dto = new PostEmailDto
        {
            Name = "Alice Smith",
            Attending = "Yes",
            BringPartner = "Yes",
            NumberOfGuestsOvernight = 2,
            NumberOfMeatMenus = 2,
            NumberOfVegetarianMenus = 0,
            NumberOfBrunchGuests = 2,
            AdditionalInformation = "Excited to celebrate with you!"
        };

        _emailServiceMock.Setup(x => x.SendRsvpConfirmationAsync(It.IsAny<EmailTemplate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResponse.CreateSuccess());

        // Act
        var response = await client.PostAsJsonAsync("/api/notifications", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _emailServiceMock.Verify(x => x.SendRsvpConfirmationAsync(
            It.Is<EmailTemplate>(t => 
                t.Name == dto.Name && 
                t.Attending == dto.Attending &&
                t.BringPartner == dto.BringPartner &&
                t.NumberOfGuestsOvernight == dto.NumberOfGuestsOvernight &&
                t.NumberOfMeatMenus == dto.NumberOfMeatMenus &&
                t.NumberOfVegetarianMenus == dto.NumberOfVegetarianMenus &&
                t.AdditionalInformation == dto.AdditionalInformation), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailNotification_WithValidApiKey_WhenEmailServiceFails_ReturnsProblem()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        
        var dto = new PostEmailDto
        {
            Name = "Bob Johnson",
            Attending = "No",
            BringPartner = "No",
            NumberOfGuestsOvernight = 0,
            NumberOfMeatMenus = 0,
            NumberOfVegetarianMenus = 0,
            NumberOfBrunchGuests = 0,
            AdditionalInformation = "Sorry, cannot attend"
        };

        _emailServiceMock.Setup(x => x.SendRsvpConfirmationAsync(It.IsAny<EmailTemplate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResponse.CreateFail(HttpStatusCode.BadGateway));

        // Act
        var response = await client.PostAsJsonAsync("/api/notifications", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        _emailServiceMock.Verify(x => x.SendRsvpConfirmationAsync(It.IsAny<EmailTemplate>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailNotification_WithValidApiKey_WhenEmailServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyHeader, ApiKey);
        
        var dto = new PostEmailDto
        {
            Name = "Charlie Brown",
            Attending = "Maybe",
            BringPartner = "Maybe",
            NumberOfGuestsOvernight = 1,
            NumberOfMeatMenus = 1,
            NumberOfVegetarianMenus = 0,
            NumberOfBrunchGuests = 1,
            AdditionalInformation = "Will confirm later"
        };

        _emailServiceMock.Setup(x => x.SendRsvpConfirmationAsync(It.IsAny<EmailTemplate>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Email service unavailable"));

        // Act
        var response = await client.PostAsJsonAsync("/api/notifications", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        _emailServiceMock.Verify(x => x.SendRsvpConfirmationAsync(It.IsAny<EmailTemplate>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}