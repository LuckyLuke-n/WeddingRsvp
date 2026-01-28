using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using WeddingRsvp.Api.Configurations;

namespace WeddingRsvp.Api.Services;

public class SendGridEmailService : IEmailService
{
    private ILogger<SendGridEmailService> Logger { get; }
    private SendGridClient Client { get; }
    private string TemplateId { get; }
    private string[] ToEmails { get; }
    private bool Enabled { get; }

    public SendGridEmailService(IOptions<EmailServiceConfiguration> options, ILogger<SendGridEmailService> logger)
    {
        Logger = logger;
        TemplateId = options.Value.TemplateId;
        ToEmails = options.Value.ToEmails;
        Enabled = options.Value.Enabled;

        if (!Enabled)
            Logger.LogWarning("Email sending is disabled.");

        var sendgridOptions = new SendGridClientOptions
        {
            ApiKey = options.Value.ApiKey,
        };

        Client = new SendGridClient(sendgridOptions);
    }

    public async Task SendRsvpConfirmationAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        if (!Enabled)
        {
            Logger.LogInformation("Email was not sent. Email sending is disabled.");
            return;
        }
        
        var from = new EmailAddress("no-reply@lsoftware.cloud", "wedding rsvp");
        List<EmailAddress> tos = [];

        foreach (var to in ToEmails)
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
                Logger.LogInformation("Email sent successfully.");
            else
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                Logger.LogError("Email failed to send with status code {StatusCode}. Response: {Response}", response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Email failed to send.");
        }
    }
}