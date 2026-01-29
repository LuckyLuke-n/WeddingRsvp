using WeddingRsvp.Client.Generics;

namespace WeddingRsvp.Client;

public interface INotificationClient
{
    Task<ClientResponse<ClientFailResponse>> SendNotificationAsync(RsvpGuest rsvp,
        CancellationToken cancellationToken = default);
}