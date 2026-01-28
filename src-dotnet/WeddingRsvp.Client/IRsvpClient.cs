using WeddingRsvp.Client.Generics;

namespace WeddingRsvp.Client;

public interface IRsvpClient
{
    Task<ClientResponse<RsvpGuest, ClientFailResponse>> GetRsvpAsync( Guid id, CancellationToken cancellationToken = default );
    Task<ClientResponse<IEnumerable<RsvpGuest>, ClientFailResponse>> GetAllRsvpsAsync( CancellationToken cancellationToken = default );
    Task<ClientResponse<RsvpGuest, ClientFailResponse>> UpdateRsvpAsync( RsvpGuest rsvp, bool sendMail = false, bool isAdmin = false, CancellationToken cancellationToken = default );
    Task<ClientResponse<RsvpGuest, ClientFailResponse>> AddRsvpAsync( RsvpGuest rsvp, CancellationToken cancellationToken = default );
    Task<ClientResponse<ClientFailResponse>> DeleteRsvpAsync( Guid id, CancellationToken cancellationToken = default );
}