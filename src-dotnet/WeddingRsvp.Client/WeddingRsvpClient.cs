using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using WeddingRsvp.Abstractions.Models;
using WeddingRsvp.Abstractions.Models.Rsvps;
using WeddingRsvp.Client.Extensions;
using WeddingRsvp.Client.Generics;

namespace WeddingRsvp.Client;

public class WeddingRsvpClient : IRsvpClient
{
    public static string RsvpClientName => "RsvpClient";
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

    public async Task<ClientResponse<RsvpGuest, ClientFailResponse>> UpdateRsvpAsync(RsvpGuest rsvp,
        CancellationToken cancellationToken = default)
    {
        using var client = HttpClientFactory.CreateClient(RsvpClientName);

        try
        {
            var responseMessage = await client.PutAsJsonAsync($"{rsvp.Id}", rsvp.ToDto(), cancellationToken)
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
            Logger.LogError(e, "Network error or server is unreachable while getting rsvp {Id}.", rsvp.Id);
        }
        catch (OperationCanceledException e)
        {
            Logger.LogInformation(e, "The request for rsvp {Id} timed out or was cancelled.", rsvp.Id);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An unexpected error occurred while getting rsvp {Id}.", rsvp.Id);
        }

        return ClientResponse<RsvpGuest, ClientFailResponse>.CreateFail(new(HttpStatusCode.InternalServerError));
    }
}