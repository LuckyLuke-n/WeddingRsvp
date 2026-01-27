using WeddingRsvp.Abstractions.Models.Rsvps;

namespace WeddingRsvp.Api.Services;

public interface IEmailService
{
    Task SendRsvpConfirmationAsync(EmailTemplate template, CancellationToken cancellationToken = default);
}