using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimGet.Services;

namespace SlimGet.Filters
{
    public sealed class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        public const string AuthenticationSchemeName = "TokenAuthenticationScheme";

        public const string ClaimTypeIssueTimestamp = "https://nuget.emzi0767.com/security-schemas/2019/07/identity/claims/issuetimestamp";
        public const string ClaimTypeGuid = "https://nuget.emzi0767.com/security-schemas/2019/07/identity/claims/guid";

        private SlimGetContext Database { get; }
        private TokenService Tokens { get; }
        private ILogger<TokenAuthenticationHandler> LocalLogger { get; }

        public TokenAuthenticationHandler(IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, SlimGetContext database, TokenService tokens)
            : base(options, logger, encoder, clock)
        {
            this.Database = database;
            this.Tokens = tokens;
            this.LocalLogger = logger.CreateLogger<TokenAuthenticationHandler>();
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!this.Request.Headers.TryGetValue("X-NuGet-ApiKey", out var values) || values.Count == 0)
                return Task.FromResult(AuthenticateResult.Fail("Missing API key"));

            var tokenStr = values.First();
            if (!this.Tokens.TryReadTokenId(tokenStr, out var guid))
            {
                this.LocalLogger.LogWarning("Failed to authenticate using invalid token '{0}'", tokenStr);
                return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));
            }

            var tokenData = this.Database.Tokens.FirstOrDefault(x => x.Guid == guid);
            if (tokenData == null || !this.Tokens.ValidateToken(tokenStr, tokenData.UserId, tokenData.IssuedAt.Value, guid, out var token))
            {
                this.LocalLogger.LogWarning("Failed to authenticate using nonexistent token {0:B}", guid);
                return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, token.UserId),
                new Claim(ClaimTypes.Name, token.UserId),
                new Claim(ClaimTypeIssueTimestamp, token.IssuedAt.ToUnixTimeMilliseconds().ToString()),
                new Claim(ClaimTypeGuid, token.Guid.ToString("N"))
            };
            var identity = new ClaimsIdentity(claims, this.Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

            this.LocalLogger.LogInformation("Authenticated user '{0}' via token {1:B}", token.UserId, token.Guid);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
