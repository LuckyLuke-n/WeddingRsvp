using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeddingRsvp.Abstractions.Models.Settings;
using WeddingRsvp.Api.Configurations;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Services;

namespace WeddingRsvp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ApiKeyPolicy")]
public class SettingsController : Controller
{
    private ILogger<SettingsController> Logger { get; }
    private ISettingsService Service { get; }
    private ApiConfiguration Configurations { get; }

    public SettingsController(ILogger<SettingsController> logger,
        ISettingsService service,
        IOptions<ApiConfiguration> options)
    {
        Logger = logger;
        Service = service;
        Configurations = options.Value;
    }
    
    [HttpGet("")]
    public async Task<IResult> Get( CancellationToken cancellationToken = default )
    {
        var response = await Service.GetAsync( cancellationToken ).ConfigureAwait( false );

        if (!response.IsSuccess)
            return Results.InternalServerError();

        List<GetSettingsDto> dtos = [response.ValueSuccess!.ToDto()];
        
        return Results.Ok(dtos);
    }

    [HttpPut("")]
    public async Task<IResult> Update( PutSettingsDto dto, CancellationToken cancellationToken = default)
    {
        var settings = dto.ToEntity();
        var response = await Service.UpsertAsync(settings, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccess)
            return Results.InternalServerError();

        return Results.Ok();
    }
}