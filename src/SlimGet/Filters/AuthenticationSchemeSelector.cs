// This file is a part of SlimGet project.
//
// Copyright 2019 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
