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
            AuthenticationHandlerSelector.HandleChallenge(this.Context);
            return Task.CompletedTask;
        }
    }
}
