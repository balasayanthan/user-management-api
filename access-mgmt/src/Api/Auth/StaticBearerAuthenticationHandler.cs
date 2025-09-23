using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Api.Auth
{
    public sealed class StaticBearerAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ISystemClock clock,
    IOptions<StaticBearerOptions> staticOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder, clock)
    {
        public const string Scheme = "StaticBearer";
        private readonly StaticBearerOptions _opts = staticOptions.Value;

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Expect: Authorization: Bearer <token>
            if (!Request.Headers.TryGetValue("Authorization", out var auth) || auth.Count == 0)
                return Task.FromResult(AuthenticateResult.NoResult());

            var value = auth.ToString();
            if (!value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(AuthenticateResult.NoResult());

            var token = value.Substring("Bearer ".Length).Trim();
            var entry = _opts.Tokens.FirstOrDefault(t => string.Equals(t.Token, token, StringComparison.Ordinal));
            if (entry is null)
                return Task.FromResult(AuthenticateResult.Fail("Invalid bearer token"));

            var claims = new List<Claim>
        {
            new(ClaimTypes.Name, entry.Name),
            new("auth_scheme", Scheme),
        };

            // Map "perm:XYZ" strings to claim type "permission" (value = XYZ)
            foreach (var c in entry.Claims)
            {
                var parts = c.Split(':', 2);
                if (parts.Length == 2 && parts[0].Equals("perm", StringComparison.OrdinalIgnoreCase))
                    claims.Add(new Claim("permission", parts[1]));
                else
                    claims.Add(new Claim("custom", c));
            }

            var identity = new ClaimsIdentity(claims, Scheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
