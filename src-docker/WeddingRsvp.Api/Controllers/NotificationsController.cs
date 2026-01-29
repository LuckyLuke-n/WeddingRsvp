using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeddingRsvp.Abstractions.Models.Notifications;
using WeddingRsvp.Api.Extensions;
using WeddingRsvp.Api.Services;

namespace WeddingRsvp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ApiKeyPolicy")]
public class NotificationsController : Controller
{
    private IEmailService EmailService { get; }
    private ILogger<NotificationsController> Logger { get; }

    public NotificationsController( IEmailService emailService, ILogger<NotificationsController> logger )
    {
        EmailService = emailService;
        Logger = logger;
    }
    
    [HttpPost("")]
    public async Task<IResult> SendEmailNotification( PostEmailDto dto, CancellationToken cancellationToken = default )
    {
        var response = await EmailService.SendRsvpConfirmationAsync(dto.ToEmailTemplate(), cancellationToken)
            .ConfigureAwait(false);
        
        if (!response.IsSuccess)
        {
            Logger.LogError("Email notification not sent with status code {StatusCode}.", response.StatusCode);
            return Results.Problem( detail: "Email notification not sent.", statusCode: (int)response.StatusCode);
        }
        
        return Results.Ok();
    }
}