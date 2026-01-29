using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using WeddingRsvp.Abstractions.Models.Information;
using WeddingRsvp.Abstractions.Models.Rsvps;
using WeddingRsvp.Client.Extensions;
using WeddingRsvp.Client.Generics;

namespace WeddingRsvp.Client;

public class WeddingRsvpClient : IRsvpClient, IInformationClient, INotificationClient
{
    public static string RsvpClientName => "RsvpClient";
    public static string RsvpAdminClientName => "RsvpAdminClient";
    public static string InformationClientName => "InformationClient";
    public static string InformationAdminClientName => "InformationAdminClient";
    public static string NotificationClientName => "NotificationClient";
    
    private IHttpClientFactory HttpClientFactory { get; }
    private ILogger<WeddingRsvpClient> Logger { get; }

    public WeddingRsvpClient(IHttpClientFactory httpClientFactory,
        ILogger<WeddingRsvpClient> logger)
    {
        HttpClientFactory = httpClientFactory;
        Logger = logger;
    }

    public async Task<ClientResponse<RsvpGuest, ClientFailResponse>> GetRsvpAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        using var client = HttpClientFactory.CreateClient(RsvpClientName);

        try
        {
            var responseMessage = await client.GetAsync($"{id}", cancellationToken).ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
                return ClientResponse<RsvpGuest, ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));

            var dto = await responseMessage.Content.ReadFromJsonAsync<GetRsvpDto>(cancellationToken: cancellationToken);

            if (dto is null)
            {
                Logger.LogError("Cannot deserialize response from {ClientName} to {DtoType}.", RsvpClientName,
                    typeof(GetRsvpDto));
                return ClientResponse<RsvpGuest, ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));
            }

            var rsvp = dto.ToDomainObject();
            return ClientResponse<RsvpGuest, ClientFailResponse>.CreateSuccess(rsvp);
        }
        catch (HttpRequestException e)
        {
            Logger.LogError(e, "Network error or server is unreachable while getting rsvp {Id}.", id);
        }
        catch (OperationCanceledException e)
        {
            Logger.LogInformation(e, "The request for rsvp {Id} timed out or was cancelled.", id);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred while getting rsvp {Id}.", id);
        }

        return ClientResponse<RsvpGuest, ClientFailResponse>.CreateFail(new(HttpStatusCode.InternalServerError));
    }

    public async Task<ClientResponse<IEnumerable<RsvpGuest>, ClientFailResponse>> GetAllRsvpsAsync(CancellationToken cancellationToken = default)
    {
        using var client = HttpClientFactory.CreateClient(RsvpAdminClientName);
        
        try
        {
            var responseMessage = await client.GetAsync("", cancellationToken).ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
                return ClientResponse<IEnumerable<RsvpGuest>, ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));

            var dto = await responseMessage.Content.ReadFromJsonAsync<IEnumerable<GetRsvpDto>>(cancellationToken);

            if (dto is null)
            {
                Logger.LogError("Cannot deserialize response from {ClientName} to {DtoType}.", InformationClientName,
                    typeof(IEnumerable<RsvpGuest>));
                return ClientResponse<IEnumerable<RsvpGuest>, ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));
            }

            var rsvps = dto.Select( i => i.ToDomainObject());
            return ClientResponse<IEnumerable<RsvpGuest>, ClientFailResponse>.CreateSuccess(rsvps);
        }
        catch (HttpRequestException e)
        {
            Logger.LogError(e, "Network error or server is unreachable while getting information in all languages.");
        }
        catch (OperationCanceledException e)
        {
            Logger.LogInformation(e, "The request getting information in all languages timed out or was cancelled.");
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred while getting information in all languages.");
        }
        
        return ClientResponse<IEnumerable<RsvpGuest>, ClientFailResponse>.CreateFail(new(HttpStatusCode.InternalServerError));
    }

    public async Task<ClientResponse<RsvpGuest, ClientFailResponse>> UpdateRsvpAsync(RsvpGuest rsvp, bool isAdmin = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage responseMessage;
            if (isAdmin)
            {
                using var client = HttpClientFactory.CreateClient(RsvpAdminClientName);
                responseMessage = await client.PutAsJsonAsync($"{rsvp.Id}", rsvp.ToPutDto(), cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                using var client = HttpClientFactory.CreateClient(RsvpClientName); 
                responseMessage = await client.PutAsJsonAsync($"{rsvp.Id}", rsvp.ToPutDto(), cancellationToken)
                    .ConfigureAwait(false);
            }

            if (!responseMessage.IsSuccessStatusCode)
                return ClientResponse<RsvpGuest, ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));

            var dto = await responseMessage.Content.ReadFromJsonAsync<GetRsvpDto>(cancellationToken: cancellationToken);

            if (dto is not null)
                return ClientResponse<RsvpGuest, ClientFailResponse>.CreateSuccess(dto.ToDomainObject());

            Logger.LogError("Cannot deserialize response from {ClientName} to {DtoType}.", RsvpClientName, typeof(GetRsvpDto));
            return ClientResponse<RsvpGuest, ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));
        }
        catch (HttpRequestException e)
        {
            Logger.LogError(e, "Network error or server is unreachable while updating rsvp {Id}.", rsvp.Id);
        }
        catch (OperationCanceledException e)
        {
            Logger.LogInformation(e, "The request for updating rsvp {Id} timed out or was cancelled.", rsvp.Id);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred while updating rsvp {Id}.", rsvp.Id);
        }

        return ClientResponse<RsvpGuest, ClientFailResponse>.CreateFail(new(HttpStatusCode.InternalServerError));
    }

    public async Task<ClientResponse<RsvpGuest, ClientFailResponse>> AddRsvpAsync(RsvpGuest rsvp, CancellationToken cancellationToken = default)
    {
        using var client = HttpClientFactory.CreateClient(RsvpAdminClientName);

        try
        {
            var responseMessage = await client.PostAsJsonAsync("", rsvp.ToPostDto(), cancellationToken)
                .ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
                return ClientResponse<RsvpGuest, ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));

            var dto = await responseMessage.Content.ReadFromJsonAsync<GetRsvpDto>(cancellationToken: cancellationToken);

            if (dto is not null)
                return ClientResponse<RsvpGuest, ClientFailResponse>.CreateSuccess(dto.ToDomainObject());

            Logger.LogError("Cannot deserialize response from {ClientName} to {DtoType}.", RsvpClientName, typeof(GetRsvpDto));
            return ClientResponse<RsvpGuest, ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));
        }
        catch (HttpRequestException e)
        {
            Logger.LogError(e, "Network error or server is unreachable while creating rsvp {Id}.", rsvp.Id);
        }
        catch (OperationCanceledException e)
        {
            Logger.LogInformation(e, "The request for creating rsvp {Id} timed out or was cancelled.", rsvp.Id);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred while creating rsvp {Id}.", rsvp.Id);
        }

        return ClientResponse<RsvpGuest, ClientFailResponse>.CreateFail(new(HttpStatusCode.InternalServerError));
    }

    public async Task<ClientResponse<ClientFailResponse>> DeleteRsvpAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var client = HttpClientFactory.CreateClient(RsvpAdminClientName);

        try
        {
            var responseMessage = await client.DeleteAsync($"{id}", cancellationToken)
                .ConfigureAwait(false);
            
            if (!responseMessage.IsSuccessStatusCode)
                return ClientResponse<ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));

            return ClientResponse<ClientFailResponse>.CreateSuccess();
        }
        catch (HttpRequestException e)
        {
            Logger.LogError(e, "Network error or server is unreachable while deleting rsvp {Id}.", id);
        }
        catch (OperationCanceledException e)
        {
            Logger.LogInformation(e, "The request for deleting rsvp {Id} timed out or was cancelled.", id);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred while deleting rsvp {Id}.", id);
        }
        
        return ClientResponse<ClientFailResponse>.CreateFail(new(HttpStatusCode.InternalServerError));
    }

    public async Task<ClientResponse<IEnumerable<DynamicInformation>, ClientFailResponse>> GetInformationInAllLanguagesAsync(CancellationToken cancellationToken = default)
    {
        using var client = HttpClientFactory.CreateClient(InformationAdminClientName);
        
        try
        {
            var responseMessage = await client.GetAsync("", cancellationToken).ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
                return ClientResponse<IEnumerable<DynamicInformation>, ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));

            var dto = await responseMessage.Content.ReadFromJsonAsync<IEnumerable<GetInformationDto>>(cancellationToken);

            if (dto is null)
            {
                Logger.LogError("Cannot deserialize response from {ClientName} to {DtoType}.", InformationClientName,
                    typeof(IEnumerable<DynamicInformation>));
                return ClientResponse<IEnumerable<DynamicInformation>, ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));
            }

            var dynamicInformation = dto.Select( i => i.ToDomainObject());
            return ClientResponse<IEnumerable<DynamicInformation>, ClientFailResponse>.CreateSuccess(dynamicInformation);
        }
        catch (HttpRequestException e)
        {
            Logger.LogError(e, "Network error or server is unreachable while getting information in all languages.");
        }
        catch (OperationCanceledException e)
        {
            Logger.LogInformation(e, "The request getting information in all languages timed out or was cancelled.");
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred while getting information in all languages.");
        }
        
        return ClientResponse<IEnumerable<DynamicInformation>, ClientFailResponse>.CreateFail(new(HttpStatusCode.InternalServerError));
    }

    public async Task<ClientResponse<DynamicInformation, ClientFailResponse>> GetInformationAsync(string language, CancellationToken cancellationToken = default)
    {
        using var client = HttpClientFactory.CreateClient(InformationClientName);
        
        try
        {
            var responseMessage = await client.GetAsync($"language/{language}", cancellationToken).ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
                return ClientResponse<DynamicInformation, ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));

            var dto = await responseMessage.Content.ReadFromJsonAsync<GetInformationDto>(cancellationToken);

            if (dto is null)
            {
                Logger.LogError("Cannot deserialize response from {ClientName} to {DtoType}.", InformationClientName,
                    typeof(DynamicInformation));
                return ClientResponse<DynamicInformation, ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));
            }

            return ClientResponse<DynamicInformation, ClientFailResponse>.CreateSuccess(dto.ToDomainObject());
        }
        catch (HttpRequestException e)
        {
            Logger.LogError(e, "Network error or server is unreachable while getting information in all languages.");
        }
        catch (OperationCanceledException e)
        {
            Logger.LogInformation(e, "The request getting information in all languages timed out or was cancelled.");
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred while getting information in all languages.");
        }
        
        return ClientResponse<DynamicInformation, ClientFailResponse>.CreateFail(new(HttpStatusCode.InternalServerError));
    }

    public async Task<ClientResponse<DynamicInformation,ClientFailResponse>> UpdateInformationAsync(DynamicInformation information, CancellationToken cancellationToken = default)
    {
        using var client = HttpClientFactory.CreateClient(InformationAdminClientName);
        
        try
        {
            var responseMessage = await client.PutAsJsonAsync($"{information.Id}", information.ToPutDto(), cancellationToken).ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
                return ClientResponse<DynamicInformation,ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));

            var dto = await responseMessage.Content.ReadFromJsonAsync<GetInformationDto>(cancellationToken);

            if (dto is null)
            {
                Logger.LogError("Cannot deserialize response from {ClientName} to {DtoType}.", InformationClientName,
                    typeof(DynamicInformation));
                return ClientResponse<DynamicInformation,ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));
            }

            return ClientResponse<DynamicInformation,ClientFailResponse>.CreateSuccess(dto.ToDomainObject());
        }
        catch (HttpRequestException e)
        {
            Logger.LogError(e, "Network error or server is unreachable while updating information {Language}.", information.Language);
        }
        catch (OperationCanceledException e)
        {
            Logger.LogInformation(e, "The request updating information in {Language} timed out or was cancelled.", information.Language);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred while updating information in {Language}.", information.Language);
        }
        
        return ClientResponse<DynamicInformation,ClientFailResponse>.CreateFail(new(HttpStatusCode.InternalServerError));
    }

    public async Task<ClientResponse<DynamicInformation,ClientFailResponse>> AddInformationAsync(DynamicInformation information, CancellationToken cancellationToken = default)
    {
        using var client = HttpClientFactory.CreateClient(InformationAdminClientName);
        
        try
        {
            var responseMessage = await client.PostAsJsonAsync("", information.ToPostDto(), cancellationToken).ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
                return ClientResponse<DynamicInformation,ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));

            var dto = await responseMessage.Content.ReadFromJsonAsync<GetInformationDto>(cancellationToken);

            if (dto is null)
            {
                Logger.LogError("Cannot deserialize response from {ClientName} to {DtoType}.", InformationClientName,
                    typeof(DynamicInformation));
                return ClientResponse<DynamicInformation,ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));
            }

            return ClientResponse<DynamicInformation,ClientFailResponse>.CreateSuccess(dto.ToDomainObject());
        }
        catch (HttpRequestException e)
        {
            Logger.LogError(e, "Network error or server is unreachable while creating information in {Language}.", information.Language);
        }
        catch (OperationCanceledException e)
        {
            Logger.LogInformation(e, "The request creating information in {Language} timed out or was cancelled.", information.Language);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred while creating information {Language}.", information.Language);
        }
        
        return ClientResponse<DynamicInformation,ClientFailResponse>.CreateFail(new(HttpStatusCode.InternalServerError));
    }

    public async Task<ClientResponse<ClientFailResponse>> SendNotificationAsync(RsvpGuest rsvp, CancellationToken cancellationToken = default)
    {
        using var client = HttpClientFactory.CreateClient(NotificationClientName);
        
        try
        {
            var responseMessage = await client.PostAsJsonAsync("", rsvp.ToPostNotificationDto(), cancellationToken).ConfigureAwait(false);

            if ( !responseMessage.IsSuccessStatusCode )
            {
                Logger.LogWarning("Cannot send email for {RsvpId} with status code {StatusCode}.", rsvp.Id, responseMessage.StatusCode);
                return ClientResponse<ClientFailResponse>.CreateFail(new(responseMessage.StatusCode));
            }
            
            return ClientResponse<ClientFailResponse>.CreateSuccess();
        }
        catch (HttpRequestException e)
        {
            Logger.LogError(e, "Network error or server is unreachable while triggering email notification for {RsvpId}.", rsvp.Id);
        }
        catch (OperationCanceledException e)
        {
            Logger.LogInformation(e, "The request triggering email notification for {RsvpId} timed out or was cancelled.", rsvp.Id);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred while triggering email notification for {RsvpId}.", rsvp.Id);
        }
        
        return ClientResponse<ClientFailResponse>.CreateFail(new(HttpStatusCode.InternalServerError));
    }
}