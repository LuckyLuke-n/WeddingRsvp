using WeddingRsvp.Client.Generics;

namespace WeddingRsvp.Client;

public interface ISettingsClient
{
    Task<ClientResponse<ApplicationSettings, ClientFailResponse>> GetSettingsAsync( CancellationToken cancellationToken = default );
    Task<ClientResponse<ApplicationSettings, ClientFailResponse>> UpdateSettingsAsync( ApplicationSettings settings, CancellationToken cancellationToken = default );
}