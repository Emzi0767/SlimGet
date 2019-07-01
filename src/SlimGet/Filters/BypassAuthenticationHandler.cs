using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SlimGet.Filters
{
    public sealed class BypassAuthenticationHandler : AuthenticationHandler<BypassAuthenticationOptions>
    {
        public const string AuthenticationSchemeName = "BypassAuthenticationScheme";

        public BypassAuthenticationHandler(IOptionsMonitor<BypassAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            => Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(), AuthenticationSchemeName)));

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            AuthenticationSchemeSelector.HandleChallenge(this.Context);
            return Task.CompletedTask;
        }
    }
}
