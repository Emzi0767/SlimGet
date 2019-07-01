using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace SlimGet.Filters
{
    public static class AuthenticationSchemeSelector
    {
        public const string AuthenticationSchemeName = "SelectorAuthenticationScheme";

        public static void ConfigureSelector(PolicySchemeOptions opts)
            => opts.ForwardDefaultSelector = SelectPolicy;

        public static void HandleChallenge(HttpContext ctx)
        {
            if (ctx.Request.Headers.TryGetValue(HeaderNames.UserAgent, out var uas) && uas.Count > 0)
            {
                var ua = uas.First();
                if (ua.StartsWith("NuGet Command Line"))
                {
                    // NuGet CLI is autistic and treats all 401s as challenges
                    // even if you do not supply authentication method
                    ctx.Response.StatusCode = 403;
                    return;
                }
            }

            ctx.Response.StatusCode = 401;
        }

        private static string SelectPolicy(HttpContext ctx)
        {
            if (ctx.Request.Path == "/dev" || ctx.Request.Path.StartsWithSegments("/dev"))
                return BypassAuthenticationHandler.AuthenticationSchemeName;

            return TokenAuthenticationHandler.AuthenticationSchemeName;
        }
    }
}
