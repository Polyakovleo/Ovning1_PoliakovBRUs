using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BankRUs.Api.Auth;

public sealed class HeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Header";
    public const string UserIdHeader = "X-UserId";
    public const string RolesHeader = "X-Roles";
    public const string RoleHeader = "X-Role";

    public HeaderAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var hasUserId = Request.Headers.TryGetValue(UserIdHeader, out var userId);
        var hasRoles = Request.Headers.TryGetValue(RolesHeader, out var rolesCsv)
                       || Request.Headers.TryGetValue(RoleHeader, out rolesCsv);

        if (!hasUserId && !hasRoles)
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim>();

        if (hasUserId)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

        if (hasRoles)
        {
            var roles = rolesCsv.ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

