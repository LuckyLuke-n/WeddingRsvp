using WeddingRsvp.Client.Generics;

namespace WeddingRsvp.Client;

public interface IInformationClient
{
    Task<ClientResponse<IEnumerable<DynamicInformation>,ClientFailResponse>> GetInvitationInAllLanguagesAsync(CancellationToken cancellationToken = default);
}