using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Services.Generics;

namespace WeddingRsvp.Api.Services;

public interface ISettingsService
{
    Task<ServiceResponse<Settings>> GetAsync(CancellationToken cancellationToken = default);
    Task<ServiceResponse<Settings>> UpsertAsync(Settings settings, CancellationToken cancellationToken = default);
}