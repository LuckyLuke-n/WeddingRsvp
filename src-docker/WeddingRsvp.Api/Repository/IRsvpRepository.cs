using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;

namespace WeddingRsvp.Api.Repository;

public interface IRsvpRepository
{
    Task<RepositoryResponse<Rsvp, RepositoryFailResponse>> CreateAsync( Rsvp rsvp, CancellationToken cancellationToken = default );
    Task<RepositoryResponse<IEnumerable<Rsvp>, RepositoryFailResponse>> ReadAllAsync( CancellationToken cancellationToken = default );
    Task<RepositoryResponse<Rsvp, RepositoryFailResponse>> ReadAsync( Guid id, CancellationToken cancellationToken = default );
    Task<RepositoryResponse<Rsvp, RepositoryFailResponse>> UpdateAsync( Rsvp rsvp, CancellationToken cancellationToken = default );
    Task<RepositoryResponse<RepositoryFailResponse>> DeleteAsync( Guid id, CancellationToken cancellationToken = default );
}