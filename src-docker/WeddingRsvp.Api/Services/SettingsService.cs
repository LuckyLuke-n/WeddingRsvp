using System.Net;
using WeddingRsvp.Api.Repository;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Services.Generics;

namespace WeddingRsvp.Api.Services;

public class SettingsService : ISettingsService
{
    private ISettingsRepository Repository { get; }
    private ILogger<SettingsService> Logger { get; }
    public static Guid SettingsId => Guid.Parse("4404ffef-d848-4cfc-9ec4-3bc841db4c13");

    public SettingsService(ISettingsRepository repository, ILogger<SettingsService> logger)
    {
        Repository = repository;
        Logger = logger;
    }

    public async Task<ServiceResponse<Settings>> GetAsync(CancellationToken cancellationToken = default)
    {
        var response = await Repository.ReadAsync(SettingsId, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccess)
        {
            switch (response.ValueFail.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    Settings newSettings = new() { Id = SettingsId.ToString() };
                    var createResponse = await Repository.CreateAsync(newSettings, cancellationToken)
                        .ConfigureAwait(false);
                    if (!createResponse.IsSuccess)
                    {
                        Logger.LogError("Cannot create settings with error: {ErrorMessage}.",
                            createResponse.ValueFail.Message);
                        return ServiceResponse<Settings>.CreateFail(createResponse.ValueFail.StatusCode);
                    }

                    return ServiceResponse<Settings>.CreateSuccess(createResponse.ValueSuccess!);
                default:
                    Logger.LogError("Cannot get settings with error: {ErrorMessage}.", response.ValueFail.Message);
                    return ServiceResponse<Settings>.CreateFail(response.ValueFail.StatusCode);
            }
        }

        return ServiceResponse<Settings>.CreateSuccess(response.ValueSuccess!);
    }

    public async Task<ServiceResponse<Settings>> UpsertAsync(Settings settings,
        CancellationToken cancellationToken = default)
    {
        settings.Id = SettingsId.ToString();
        var response = await Repository.UpdateAsync(settings, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccess)
        {
            Logger.LogError("Cannot update settings with error: {ErrorMessage}.", response.ValueFail.Message);
            // repository has upsert behavior. there is no NotFound in that case
            return ServiceResponse<Settings>.CreateFail(response.ValueFail.StatusCode);
        }

        return ServiceResponse<Settings>.CreateSuccess(settings);
    }
}