using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using WeddingRsvp.Abstractions.Models.Information;
using WeddingRsvp.Abstractions.Models.Rsvps;
using WeddingRsvp.Abstractions.Models.Settings;
using WeddingRsvp.Client;

namespace WeddingRsvp.Test;

public class WeddingRsvpClientTests
{
   private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
   private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly WeddingRsvpClient _client;

    public WeddingRsvpClientTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var loggerMock = new Mock<ILogger<WeddingRsvpClient>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _client = new WeddingRsvpClient(_httpClientFactoryMock.Object, loggerMock.Object);
    }

    private HttpClient CreateMockHttpClient(HttpResponseMessage response)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        return new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5000/")
        };
    }

    #region RSVP Tests

    [Fact]
    public async Task GetRsvpAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var rsvpId = Guid.NewGuid();
        var dto = new GetRsvpDto
        {
            Id = rsvpId.ToString(),
            Name = "John Doe",
            Salutation = "Dear John",
            Attending = Reply.Yes,
            BringPartner = Reply.No,
            NumberOfGuestsOvernight = 2,
            NumberOfMeatMenus = 1,
            NumberOfBrunchGuests = 0,
            NumberOfVegetarianMenus = 1,
            AdditionalInformation = "None",
            IsPlural = false
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(dto)
        };

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.RsvpClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.GetRsvpAsync(rsvpId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ValueSuccess);
        Assert.Equal(rsvpId.ToString(), result.ValueSuccess.Id);
        Assert.Equal("John Doe", result.ValueSuccess.Name);
    }

    [Fact]
    public async Task GetRsvpAsync_WithNotFound_ReturnsFail()
    {
        // Arrange
        var rsvpId = Guid.NewGuid();
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.RsvpClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.GetRsvpAsync(rsvpId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.ValueFail.StatusCode);
    }

    [Fact]
    public async Task GetAllRsvpsAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dtos = new List<GetRsvpDto>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Alice",
                Salutation = "Dear Alice",
                Attending = Reply.Yes,
                BringPartner = Reply.Yes,
                IsPlural = false
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Bob",
                Salutation = "Dear Bob",
                Attending = Reply.No,
                BringPartner = Reply.No,
                IsPlural = false
            }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(dtos)
        };

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.RsvpAdminClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.GetAllRsvpsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ValueSuccess);
        Assert.Equal(2, result.ValueSuccess.Count());
    }

    [Fact]
    public async Task UpdateRsvpAsync_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var rsvpGuest = new RsvpGuest
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Jane Doe",
            Salutation = "Dear Jane",
            Response = ResponseType.Yes,
            BringPartner = ResponseType.No,
            NumberOfGuestsOvernight = 1,
            IsPlural = false
        };

        var responseDto = new GetRsvpDto
        {
            Id = rsvpGuest.Id,
            Name = rsvpGuest.Name,
            Salutation = rsvpGuest.Salutation,
            Attending = Reply.Yes,
            BringPartner = Reply.No,
            NumberOfGuestsOvernight = 1,
            IsPlural = false
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.RsvpAdminClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.UpdateRsvpAsync(rsvpGuest, isAdmin: true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ValueSuccess);
        Assert.Equal(rsvpGuest.Name, result.ValueSuccess.Name);
    }

    [Fact]
    public async Task UpdateRsvpAsync_AsUser_ReturnsSuccess()
    {
        // Arrange
        var rsvpGuest = new RsvpGuest
        {
            Id = Guid.NewGuid().ToString(),
            Name = "User Test",
            Salutation = "Dear User",
            Response = ResponseType.Yes,
            IsPlural = false
        };

        var responseDto = new GetRsvpDto
        {
            Id = rsvpGuest.Id,
            Name = rsvpGuest.Name,
            Salutation = rsvpGuest.Salutation,
            Attending = Reply.Yes,
            IsPlural = false
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.RsvpClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.UpdateRsvpAsync(rsvpGuest, isAdmin: false);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ValueSuccess);
    }

    [Fact]
    public async Task AddRsvpAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var newGuest = new RsvpGuest
        {
            Name = "New Guest",
            Salutation = "Dear Guest",
            IsPlural = false
        };

        var responseDto = new GetRsvpDto
        {
            Id = Guid.NewGuid().ToString(),
            Name = newGuest.Name,
            Salutation = newGuest.Salutation,
            IsPlural = false,
            Attending = Reply.None,
            BringPartner = Reply.None
        };

        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(responseDto)
        };

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.RsvpAdminClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.AddRsvpAsync(newGuest);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ValueSuccess);
        Assert.Equal(newGuest.Name, result.ValueSuccess.Name);
        Assert.NotEmpty(result.ValueSuccess.Id);
    }

    [Fact]
    public async Task DeleteRsvpAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var rsvpId = Guid.NewGuid();
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.RsvpAdminClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.DeleteRsvpAsync(rsvpId);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteRsvpAsync_WithNotFound_ReturnsFail()
    {
        // Arrange
        var rsvpId = Guid.NewGuid();
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.RsvpAdminClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.DeleteRsvpAsync(rsvpId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.ValueFail.StatusCode);
    }

    #endregion

    #region Information Tests

    [Fact]
    public async Task GetInformationInAllLanguagesAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dtos = new List<GetInformationDto>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Language = "en",
                InvitationText = "Welcome!",
                Itinerary = new Dictionary<string, string> { { "10:00", "Ceremony" } },
                Faqs = new Dictionary<string, string> { { "Where?", "At the church" } }
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Language = "de",
                InvitationText = "Willkommen!",
                Itinerary = new Dictionary<string, string> { { "10:00", "Zeremonie" } },
                Faqs = new Dictionary<string, string> { { "Wo?", "In der Kirche" } }
            }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(dtos)
        };

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.InformationAdminClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.GetInformationInAllLanguagesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ValueSuccess);
        Assert.Equal(2, result.ValueSuccess.Count());
    }

    [Fact]
    public async Task GetInformationAsync_WithValidLanguage_ReturnsSuccess()
    {
        // Arrange
        var language = "en";
        var dto = new GetInformationDto
        {
            Id = Guid.NewGuid().ToString(),
            Language = language,
            InvitationText = "Welcome to our wedding!",
            Itinerary = new Dictionary<string, string> { { "14:00", "Reception" } },
            Faqs = new Dictionary<string, string> { { "Dress code?", "Formal" } }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(dto)
        };

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.InformationClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.GetInformationAsync(language);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ValueSuccess);
        Assert.Equal(language, result.ValueSuccess.Language);
        Assert.Equal("Welcome to our wedding!", result.ValueSuccess.InvitationText);
    }

    [Fact]
    public async Task UpdateInformationAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var information = new DynamicInformation
        {
            Id = Guid.NewGuid().ToString(),
            Language = "en",
            InvitationText = "Updated text",
            Itinerary = new Dictionary<string, string> { { "15:00", "Dinner" } },
            Faqs = new Dictionary<string, string> { { "Parking?", "Available" } }
        };

        var responseDto = new GetInformationDto
        {
            Id = information.Id,
            Language = information.Language,
            InvitationText = information.InvitationText,
            Itinerary = information.Itinerary,
            Faqs = information.Faqs
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.InformationAdminClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.UpdateInformationAsync(information);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ValueSuccess);
        Assert.Equal(information.InvitationText, result.ValueSuccess.InvitationText);
    }

    [Fact]
    public async Task AddInformationAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var newInfo = new DynamicInformation
        {
            Language = "fr",
            InvitationText = "Bienvenue!",
            Itinerary = new Dictionary<string, string> { { "16:00", "Dîner" } },
            Faqs = new Dictionary<string, string> { { "Où?", "À l'église" } }
        };

        var responseDto = new GetInformationDto
        {
            Id = Guid.NewGuid().ToString(),
            Language = newInfo.Language,
            InvitationText = newInfo.InvitationText,
            Itinerary = newInfo.Itinerary,
            Faqs = newInfo.Faqs
        };

        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(responseDto)
        };

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.InformationAdminClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.AddInformationAsync(newInfo);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ValueSuccess);
        Assert.Equal(newInfo.Language, result.ValueSuccess.Language);
        Assert.NotEmpty(result.ValueSuccess.Id);
    }

    [Fact]
    public async Task GetInformationAsync_WithInvalidLanguage_ReturnsFail()
    {
        // Arrange
        var language = "invalid";
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.InformationClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.GetInformationAsync(language);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.ValueFail.StatusCode);
    }
    
    #endregion

    #region Settings Tests

    [Fact]
    public async Task GetSettingsAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new GetSettingsDto
        {
            EnableEmailNotifications = true,
            EmailRecipients = ["test@example.com"],
            RespondUntil = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };
        
        List<GetSettingsDto> dtos = [dto];

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(dtos)
        };

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.SettingsClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.GetSettingsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ValueSuccess);
        Assert.Equal(dto.EnableEmailNotifications, result.ValueSuccess.EnableEmailNotifications);
        Assert.Equal(dto.EmailRecipients, result.ValueSuccess.EmailRecipients);
        Assert.Equal(dto.RespondUntil, result.ValueSuccess.RespondUntil);
    }

    [Fact]
    public async Task GetSettingsAsync_WithNotFound_ReturnsFail()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.SettingsClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.GetSettingsAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.ValueFail.StatusCode);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var settings = new ApplicationSettings
        {
            EnableEmailNotifications = false,
            EmailRecipients = ["updated@example.com"],
            RespondUntil = new DateTime(2031, 2, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        var responseDto = new GetSettingsDto
        {
            EnableEmailNotifications = settings.EnableEmailNotifications,
            EmailRecipients = settings.EmailRecipients,
            RespondUntil = settings.RespondUntil
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(responseDto)
        };

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.SettingsClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.UpdateSettingsAsync(settings);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ValueSuccess);
        Assert.Equal(settings.EnableEmailNotifications, result.ValueSuccess.EnableEmailNotifications);
        Assert.Equal(settings.EmailRecipients, result.ValueSuccess.EmailRecipients);
        Assert.Equal(settings.RespondUntil, result.ValueSuccess.RespondUntil);
    }

    [Fact]
    public async Task UpdateSettingsAsync_WithServerError_ReturnsFail()
    {
        // Arrange
        var settings = new ApplicationSettings
        {
            EnableEmailNotifications = true,
            EmailRecipients = ["test@example.com"],
            RespondUntil = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        var httpClient = CreateMockHttpClient(response);
        _httpClientFactoryMock.Setup(x => x.CreateClient(WeddingRsvpClient.SettingsClientName))
            .Returns(httpClient);

        // Act
        var result = await _client.UpdateSettingsAsync(settings);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.ValueFail.StatusCode);
    }

    #endregion
}