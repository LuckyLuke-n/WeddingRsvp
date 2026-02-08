using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeddingRsvp.Abstractions.Models.Settings;
using WeddingRsvp.Api.Configurations;
using WeddingRsvp.Api.Repository;
using WeddingRsvp.Api.Repository.Entities;

namespace WeddingRsvp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ApiKeyPolicy")]
public class SettingsController : Controller
{
    private ILogger<SettingsController> Logger { get; }
    private ISettingsRepository Repository { get; }
    private ApiConfiguration Configurations { get; }

    public SettingsController(ILogger<SettingsController> logger,
        ISettingsRepository repository,
        IOptions<ApiConfiguration> options)
    {
        Logger = logger;
        Repository = repository;
        Configurations = options.Value;
    }
    
    [HttpGet("{id}")]
    public async Task<IResult> Get( Guid id,  CancellationToken cancellationToken = default )
    {
        var response = await Repository.ReadAsync( id, cancellationToken ).ConfigureAwait( false );

        if (!response.IsSuccess)
        {
            Logger.LogError("Cannot get settings with error: {ErrorMessage}.", response.ValueFail.Message);
            return Results.InternalServerError();
        }

        List<GetSettingsDto> dtos = [response.ValueSuccess!.ToDto()];
        
        return Results.Ok(dtos);
    }

    [HttpPut("{id}")]
    public async Task<IResult> Update(Guid id, PutSettingsDto dto, CancellationToken cancellationToken = default)
    {
        var settings = dto.ToEntity();
        settings.Id = id.ToString();
        
        var response = await Repository.UpdateAsync(settings, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccess)
        {
            Logger.LogError("Cannot update settings with error: {ErrorMessage}.", response.ValueFail.Message);
            // repository has upsert behavior. there is no NotFound in that case
            return Results.InternalServerError();
        }

        return Results.Ok();
    }
}