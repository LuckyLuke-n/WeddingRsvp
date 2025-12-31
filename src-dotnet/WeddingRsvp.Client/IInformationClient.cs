using WeddingRsvp.Client.Generics;

namespace WeddingRsvp.Client;

public interface IInformationClient
{
    Task<ClientResponse<IEnumerable<DynamicInformation>,ClientFailResponse>> GetInAllLanguagesAsync(CancellationToken cancellationToken = default);
    Task<ClientResponse<DynamicInformation,ClientFailResponse>> GetAsync(string language, CancellationToken cancellationToken = default);
}