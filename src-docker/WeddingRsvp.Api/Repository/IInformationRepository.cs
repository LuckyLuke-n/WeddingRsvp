using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;

namespace WeddingRsvp.Api.Repository;

public interface IInformationRepository
{
    Task<RepositoryResponse<Information, RepositoryFailResponse>> CreateAsync( Information rsvp, CancellationToken cancellationToken = default );
    Task<RepositoryResponse<IEnumerable<Information>, RepositoryFailResponse>> ReadAllAsync( CancellationToken cancellationToken = default );
    Task<RepositoryResponse<Information, RepositoryFailResponse>> ReadAsync( Guid id, CancellationToken cancellationToken = default );
    Task<RepositoryResponse<Information, RepositoryFailResponse>> UpdateAsync( Information rsvp, CancellationToken cancellationToken = default );
    Task<RepositoryResponse<RepositoryFailResponse>> DeleteAsync( Guid id, CancellationToken cancellationToken = default );
}