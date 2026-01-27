using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeddingRsvp.Abstractions.Models;
using WeddingRsvp.Abstractions.Models.Rsvps;
using WeddingRsvp.Api.Configurations;
using WeddingRsvp.Api.Extensions;
using WeddingRsvp.Api.Repository;
using WeddingRsvp.Api.Repository.Entities;
using WeddingRsvp.Api.Repository.Seeding;
using WeddingRsvp.Api.Services;

namespace WeddingRsvp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ApiKeyPolicy")]
public class RsvpsController : Controller
{
    private IRsvpRepository Repository { get; }
    private RsvpSeeder Seeder { get; }
    private IEmailService EmailService { get; }
    private ApiConfiguration Configurations { get; }
    private ILogger<RsvpsController> Logger { get; }

    public RsvpsController(IRsvpRepository repository,
        RsvpSeeder seeder,
        IEmailService emailService,
        IOptions<ApiConfiguration> options,
        ILogger<RsvpsController> logger)
    {
        Repository = repository;
        Seeder = seeder;
        EmailService = emailService;
        Configurations = options.Value;
        Logger = logger;
    }

    [HttpGet("")]
    public async Task<IResult> GetAll([FromHeader(Name = "X-Auth-Admin")] string? value,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(value))
            return Results.Forbid();

        var response = await Repository.ReadAllAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccess)
        {
            Logger.LogError("Cannot get all rsvps with error: {Error}.", response.ValueFail.Message);
            return Results.InternalServerError();
        }

        var rsvps = response.ValueSuccess!;
        var dto = rsvps.Select(r => r.ToDto());
        return Results.Ok(dto);
    }

    [HttpGet("{id}", Name = "GetRsvp")]
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
                    Logger.LogError("Cannot get rsvp with error: {ErrorMessage}.", response.ValueFail.Message);
                    return Results.InternalServerError();
            }
        }

        var rsvp = response.ValueSuccess!;
        return Results.Ok(rsvp.ToDto());
    }

    [HttpPost("")]
    public async Task<IResult> Create([FromHeader(Name = "X-Auth-Admin")] string? value, PostRsvpDto dto,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(value))
            return Results.Forbid();

        var response = await Repository.CreateAsync(dto.ToEntity(), cancellationToken).ConfigureAwait(false);

        if (response.IsSuccess)
        {
            var createdRsvp = response.ValueSuccess!;
            return Results.CreatedAtRoute("GetRsvp", new { id = createdRsvp.Id }, createdRsvp.ToDto());
        }
        else
        {
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
        else
        {
            var failedResponse = response.ValueFail;
            switch (failedResponse.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return Results.NotFound();
                default:
                    return Results.InternalServerError();
            }
        }
    }

    [HttpPut("{id}")]
    public async Task<IResult> Update([FromRoute] Guid id, [FromHeader(Name = "X-Auth-Admin")] string? value,
        [FromQuery] bool sendMail, PutRsvpDto dto, CancellationToken cancellationToken)
    {
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

        var existingRsvp = responseRead.ValueSuccess!;
        var updatedRsvp = dto.ToEntity();
        updatedRsvp.Id = existingRsvp.Id;

        if (AuthorizationNeeded(existingRsvp, updatedRsvp))
        {
            if (!IsAuthorized(value))
                return Results.Forbid();
        }

        var responseUpdate = await Repository.UpdateAsync(updatedRsvp, cancellationToken).ConfigureAwait(false);
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

        if (sendMail)
            await EmailService.SendRsvpConfirmationAsync(dto.ToEmailTemplate(), cancellationToken).ConfigureAwait(false);
        
        return Results.Ok(responseUpdate.ValueSuccess!.ToDto());
    }

#if DEBUG
    [HttpPost("seed")]
    public async Task<IResult> Seed(CancellationToken cancellationToken)
    {
        await Seeder.RunAsync(false, cancellationToken).ConfigureAwait(false);
        return Results.Ok();
    }
    
    [HttpPost("clean")]
    public async Task<IResult> Clean(CancellationToken cancellationToken)
    {
        await Seeder.RunAsync(true, cancellationToken).ConfigureAwait(false);
        return Results.Ok();
    }
#endif

    private bool IsAuthorized(string? value)
    {
        if (value is null || !string.Equals(value, Configurations.AdminIdentifier))
            return false;

        return true;
    }

    private bool AuthorizationNeeded(Rsvp existingEntity, Rsvp incomingEntity)
    {
        if (!string.Equals(existingEntity.Name, incomingEntity.Name)
            || !string.Equals(existingEntity.Salutation, incomingEntity.Salutation)
            || existingEntity.IsPlural != incomingEntity.IsPlural)
        {
            return true;
        }

        return false;
    }
}