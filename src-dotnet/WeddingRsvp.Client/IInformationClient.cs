using WeddingRsvp.Client.Generics;

namespace WeddingRsvp.Client;

public interface IInformationClient
{
    Task<ClientResponse<IEnumerable<DynamicInformation>, ClientFailResponse>> GetInformationInAllLanguagesAsync(CancellationToken cancellationToken = default);

    Task<ClientResponse<DynamicInformation, ClientFailResponse>> GetInformationAsync(string language, CancellationToken cancellationToken = default);

    Task<ClientResponse<DynamicInformation, ClientFailResponse>> UpdateInformationAsync(DynamicInformation information, CancellationToken cancellationToken = default);

    Task<ClientResponse<DynamicInformation, ClientFailResponse>> AddInformationAsync(DynamicInformation information, CancellationToken cancellationToken = default);
}