using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using WeddingRsvp.Api.Configurations;

namespace WeddingRsvp.Api.Security;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public static string SchemeName => "ApiKey";
    private string ApiKey { get; set; }

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<ApiConfiguration> apiConfigOptions)
        : base(options, logger, encoder)
    {
        ApiKey = apiConfigOptions.Value.ApiKey;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKeyValue) || !string.Equals(ApiKey, apiKeyValue))
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));

        var claims = new[] { new Claim(ClaimTypes.Name, "ApiKeyUser") };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}