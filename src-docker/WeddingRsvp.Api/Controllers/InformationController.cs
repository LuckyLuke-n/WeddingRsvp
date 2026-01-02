using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeddingRsvp.Abstractions.Models.Information;
using WeddingRsvp.Api.Configurations;
using WeddingRsvp.Api.Repository;
using WeddingRsvp.Api.Repository.Entities;

namespace WeddingRsvp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ApiKeyPolicy")]
public class InformationController : Controller
{
    private IInformationRepository Repository { get; }
    private ApiConfiguration Configurations { get; }
    private ILogger<InformationController> Logger { get; }

    public InformationController(IInformationRepository repository,
        IOptions<ApiConfiguration> options,
        ILogger<InformationController> logger)
    {
        Repository = repository;
        Configurations = options.Value;
        Logger = logger;
    }

    [HttpGet("")]
    public async Task<IResult> GetAll([FromHeader(Name = "X-Auth-Admin")] string? value, CancellationToken cancellationToken)
    {
        if (!IsAuthorized(value))
            return Results.Forbid();
        
        var response = await Repository.ReadAllAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccess)
        {
            Logger.LogError("Cannot get all information with error: {Error}.", response.ValueFail.Message);
            return Results.InternalServerError();
        }

        var information = response.ValueSuccess!;
        var dto = information.Select(item =>
        {
            item.SortItinerary();
            return item.ToDto();
        });
        return Results.Ok(dto);
    }

    [HttpGet("{id}")]
    public async Task<IResult> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await Repository.ReadAsync(id, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccess)
        {
            var failedResponse = response.ValueFail;
            switch (failedResponse.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return Results.NotFound();
                default:
                    Logger.LogError("Cannot get information with error: {ErrorMessage}.", response.ValueFail.Message);
                    return Results.InternalServerError();
            }
        }

        var information = response.ValueSuccess!;
        information.SortItinerary();
        return Results.Ok(information.ToDto());
    }
    
    [HttpGet("language/{language}")]
    public async Task<IResult> Get([FromRoute] string language, CancellationToken cancellationToken)
    {
        var response = await Repository.ReadByLanguageAsync(language, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccess)
        {
            var failedResponse = response.ValueFail;
            switch (failedResponse.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return Results.NotFound();
                default:
                    Logger.LogError("Cannot get information with error: {ErrorMessage}.", response.ValueFail.Message);
                    return Results.InternalServerError();
            }
        }

        var information = response.ValueSuccess!;
        information.SortItinerary();
        return Results.Ok(information.ToDto());
    }

    [HttpPost("")]
    public async Task<IResult> Create([FromHeader(Name = "X-Auth-Admin")] string? value, PostInformationDto dto,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(value))
            return Results.Forbid();

        var response = await Repository.CreateAsync(dto.ToEntity(), cancellationToken).ConfigureAwait(false);

        if (response.IsSuccess)
            return Results.Created();

        var failedResponse = response.ValueFail;
        switch (failedResponse.StatusCode)
        {
            case HttpStatusCode.Conflict:
                return Results.Conflict();
            case HttpStatusCode.BadRequest:
                return Results.BadRequest();
            default:
                return Results.InternalServerError();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IResult> Delete([FromRoute] Guid id, [FromHeader(Name = "X-Auth-Admin")] string? value,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(value))
            return Results.Forbid();

        var response = await Repository.DeleteAsync(id, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccess)
            return Results.NoContent();

        var failedResponse = response.ValueFail;
        switch (failedResponse.StatusCode)
        {
            case HttpStatusCode.NotFound:
                return Results.NotFound();
            default:
                return Results.InternalServerError();
        }
    }

    [HttpPut("{id}")]
    public async Task<IResult> Update([FromRoute] Guid id, [FromHeader(Name = "X-Auth-Admin")] string? value,
        PutInformationDto dto, CancellationToken cancellationToken)
    {
        if (!IsAuthorized(value))
            return Results.Forbid();
        
        var responseRead = await Repository.ReadAsync(id, cancellationToken).ConfigureAwait(false);

        if (!responseRead.IsSuccess)
        {
            Logger.LogError("Cannot get rsvp with error: {ErrorMessage}.", responseRead.ValueFail.Message);

            var failedResponse = responseRead.ValueFail;
            switch (failedResponse.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return Results.NotFound();
                default:
                    return Results.InternalServerError();
            }
        }

        var existingInformation = responseRead.ValueSuccess!;
        var updatedInformation = dto.ToEntity();
        updatedInformation.Id = existingInformation.Id;
        
        var responseUpdate = await Repository.UpdateAsync(updatedInformation, cancellationToken).ConfigureAwait(false);
        if (!responseUpdate.IsSuccess)
        {
            var failedResponse = responseUpdate.ValueFail;
            switch (failedResponse.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return Results.NotFound();
                default:
                    Logger.LogError("Cannot get rsvp with error: {ErrorMessage}.", responseUpdate.ValueFail.Message);
                    return Results.InternalServerError();
            }
        }

        return Results.Ok(responseUpdate.ValueSuccess!.ToDto());
    }

    private bool IsAuthorized(string? value)
    {
        if (value is null || !string.Equals(value, Configurations.AdminIdentifier))
            return false;

        return true;
    }
}