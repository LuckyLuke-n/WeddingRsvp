using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Generic;

namespace WeddingRsvp.Api.Repository;

public interface ISettingsRepository
{
    Task<RepositoryResponse<Settings, RepositoryFailResponse>> CreateAsync( Settings settings, CancellationToken cancellationToken = default );
    Task<RepositoryResponse<Settings, RepositoryFailResponse>> ReadAsync( Guid id, CancellationToken cancellationToken = default );
    Task<RepositoryResponse<Settings, RepositoryFailResponse>> UpdateAsync( Settings settings, CancellationToken cancellationToken = default );
}