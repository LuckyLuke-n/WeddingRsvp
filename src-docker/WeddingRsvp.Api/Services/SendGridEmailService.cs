using System.Net;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using WeddingRsvp.Api.Configurations;
using WeddingRsvp.Api.Services.Generics;

namespace WeddingRsvp.Api.Services;

public class SendGridEmailService : IEmailService
{
    private IServiceScopeFactory ScopeFactory { get; }
    private ILogger<SendGridEmailService> Logger { get; }
    private SendGridClient Client { get; }
    private string TemplateId { get; }

    public SendGridEmailService(IOptions<EmailServiceConfiguration> options,
        IServiceScopeFactory scopeFactory,
        ILogger<SendGridEmailService> logger)
    {
        ScopeFactory = scopeFactory;
        Logger = logger;
        TemplateId = options.Value.TemplateId;

        var sendgridOptions = new SendGridClientOptions
        {
            ApiKey = options.Value.ApiKey,
        };

        Client = new SendGridClient(sendgridOptions);
    }

    public async Task<ServiceResponse> SendRsvpConfirmationAsync(EmailTemplate template,
        CancellationToken cancellationToken = default)
    {
        using var scope = ScopeFactory.CreateScope();
        var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
        var settingsResponse = await settingsService.GetAsync(cancellationToken).ConfigureAwait(false);

        if (!settingsResponse.IsSuccess)
        {
            Logger.LogError("Failed to retrieve settings for email service.");
            return ServiceResponse.CreateFail(HttpStatusCode.InternalServerError);
        }
        
        var settings = settingsResponse.ValueSuccess!;
        
        if (!settings.EnableEmailNotifications)
        {
            Logger.LogInformation("Email was not sent. Email sending is disabled.");
            return ServiceResponse.CreateFail(HttpStatusCode.Forbidden);
        }

        var from = new EmailAddress("no-reply@lsoftware.cloud", "WeddingRsvp");
        List<EmailAddress> tos = [];

        foreach (var to in settings.EmailRecipients)
            tos.Add(new EmailAddress(to, ""));

        var message = new SendGridMessage
        {
            From = from,
            TemplateId = TemplateId
        };

        message.AddTos(tos);
        message.SetTemplateData(template);

        try
        {
            var response = await Client.SendEmailAsync(message, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                Logger.LogInformation("Email sent successfully.");
                return ServiceResponse.CreateSuccess();
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                Logger.LogError("Email failed to send with status code {StatusCode}. Response: {Response}",
                    response.StatusCode, body);
                return ServiceResponse.CreateFail(response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Email failed to send.");
            return ServiceResponse.CreateFail(HttpStatusCode.InternalServerError);
        }
    }
}