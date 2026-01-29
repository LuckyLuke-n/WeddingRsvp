using WeddingRsvp.Api.Services.Generics;

namespace WeddingRsvp.Api.Services;

public interface IEmailService
{
    Task<ServiceResponse> SendRsvpConfirmationAsync(EmailTemplate template, CancellationToken cancellationToken = default);
}