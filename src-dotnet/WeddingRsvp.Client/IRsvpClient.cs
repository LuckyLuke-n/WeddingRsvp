using WeddingRsvp.Client.Generics;

namespace WeddingRsvp.Client;

public interface IRsvpClient
{
    Task<ClientResponse<RsvpGuest, ClientFailResponse>> GetRsvpAsync( Guid id, CancellationToken cancellationToken = default );
    Task<ClientResponse<IEnumerable<RsvpGuest>, ClientFailResponse>> GetAllRsvpsAsync( CancellationToken cancellationToken = default );
    Task<ClientResponse<RsvpGuest, ClientFailResponse>> UpdateRsvpAsync( RsvpGuest rsvp, CancellationToken cancellationToken = default );
    
}