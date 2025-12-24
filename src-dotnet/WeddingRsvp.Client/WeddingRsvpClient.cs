using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using WeddingRsvp.Abstractions.Models;
using WeddingRsvp.Client.Extensions;
using WeddingRsvp.Client.Generics;

namespace WeddingRsvp.Client;

public class WeddingRsvpClient : IRsvpClient
{
    public static string RsvpClientName => "RsvpClient";
    private IHttpClientFactory HttpClientFactory { get; }
    private ILogger<WeddingRsvpClient> Logger { get; }

    public WeddingRsvpClient( IHttpClientFactory httpClientFactory,
        ILogger<WeddingRsvpClient> logger )
    {
        HttpClientFactory = httpClientFactory;
        Logger = logger;
    }
    
    public async Task<ClientResponse<RsvpGuest, ClientFailResponse>> GetRsvpAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var client = HttpClientFactory.CreateClient(RsvpClientName);

        var result = await client.GetAsync($"{id}", cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccessStatusCode)
            return ClientResponse<RsvpGuest, ClientFailResponse>.CreateFail( new( result.StatusCode) );

        var dto = await result.Content.ReadFromJsonAsync<GetRsvpDto>(cancellationToken: cancellationToken);

        if (dto is null)
        {
            Logger.LogError("Cannot deserialize response from {ClientName} to {DtoType}.", RsvpClientName, typeof(GetRsvpDto));
            return ClientResponse<RsvpGuest, ClientFailResponse>.CreateFail( new( result.StatusCode) );
        }
        
        var rsvp = dto.ToDomainObject();
        return ClientResponse<RsvpGuest, ClientFailResponse>.CreateSuccess(rsvp);
    }

    public Task<ClientResponse<RsvpGuest, ClientFailResponse>> UpdateRsvpAsync(RsvpGuest rsvp, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}