using WeddingRsvp.Client.Generics;

namespace WeddingRsvp.Client;

public interface IRsvpClient
{
    Task<ClientResponse<RsvpGuest, ClientFailResponse>> GetRsvpAsync( Guid id, CancellationToken cancellationToken = default );
}