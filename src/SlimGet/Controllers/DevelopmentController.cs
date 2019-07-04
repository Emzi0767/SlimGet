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

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using SlimGet.Data;
using SlimGet.Data.Database;
using SlimGet.Filters;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [SlimGetRoute(Routing.DevelopmentRouteName), ApiController, ServiceFilter(typeof(RequireDevelopmentEnvironment))]
    public class DevelopmentController : Controller
    {
        private SlimGetContext Database { get; }
        private RedisService Redis { get; }
        private IFileSystemService Filesystem { get; }
        private TokenService Tokens { get; }

        public DevelopmentController(SlimGetContext db, TokenService tokens, RedisService redis, IFileSystemService fs)
        {
            this.Database = db;
            this.Tokens = tokens;
            this.Redis = redis;
            this.Filesystem = fs;
        }

        // Generates a user and token for testing, unless one exists already
        [SlimGetRoute(Routing.DevelopmentTokenRouteName), HttpGet]
        public async Task<IActionResult> Token(string username = null, string email = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                username = "slimget-test";
            if (string.IsNullOrWhiteSpace(email))
                email = $"{username}@{this.HttpContext.Request.Host.Host}";

            var usr = this.Database.Users.FirstOrDefault(x => x.Id == username && x.Email == email);
            if (usr == null)
            {
                usr = new User
                {
                    Id = username,
                    Email = email
                };
                await this.Database.Users.AddAsync(usr).ConfigureAwait(false);
            }

            var tok = this.Database.Tokens.FirstOrDefault(x => x.UserId == usr.Id);
            var atok = tok != null ? new AuthenticationToken(tok.UserId, tok.IssuedAt.Value, tok.Guid) : default;
            if (tok == null)
            {
                atok = AuthenticationToken.IssueNew(usr.Id);
                tok = new Token
                {
                    UserId = atok.UserId,
                    Guid = atok.Guid,
                    IssuedAt = atok.IssuedAt.DateTime
                };
                await this.Database.Tokens.AddAsync(tok).ConfigureAwait(false);
            }

            await this.Database.SaveChangesAsync().ConfigureAwait(false);
            return this.Content(this.Tokens.EncodeToken(atok), "text/plain", Utilities.UTF8);
        }

        [SlimGetRoute(Routing.DevelopmentWhoamiRouteName), HttpGet, Authorize(AuthenticationSchemes = TokenAuthenticationHandler.AuthenticationSchemeName)]
        public IActionResult Whoami()
            => this.Json(this.HttpContext.User.Claims.ToDictionary(x => x.Type, x => x.Value));

        [SlimGetRoute(Routing.DevelopmentUrlPerComponentsRouteName), HttpGet, ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Test(string genController, string genAction)
            => this.Content(this.Url.AbsoluteUrl(genAction, genController, this.HttpContext), "text/plain", Utilities.UTF8);

        [SlimGetRoute(Routing.DevelopmentUrlPerNameRouteName), HttpGet, ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Test(string routeName)
            => this.Content(this.Url.AbsoluteUrl(routeName, this.HttpContext), "text/plain", Utilities.UTF8);

        [SlimGetRoute(Routing.DevelopmentEvalRouteName), HttpPost]
        public async Task<IActionResult> Evaluate([FromBody] string code)
        {
            var globals = new EvaluationEnvironment(this.HttpContext, this.Url, this.Database, this.Redis, this.Filesystem, this.Tokens);
            var sopts = ScriptOptions.Default
                .WithImports("System", "System.Collections.Generic", "System.Diagnostics", "System.Linq", "System.Net.Http", "System.Net.Http.Headers", "System.Reflection", "System.Text",
                             "System.Threading.Tasks", "SlimGet", "SlimGet.Data", "SlimGet.Data.Configuration", "SlimGet.Data.Database", "SlimGet.Services", "SlimGet.Models", "SlimGet.Filters")
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            var sw1 = Stopwatch.StartNew();
            var cs = CSharpScript.Create(code, sopts, typeof(EvaluationEnvironment));
            var csc = cs.Compile();
            sw1.Stop();
            this.Response.Headers.Add("X-Compilation-Time", sw1.ElapsedMilliseconds.ToString("#,##0"));

            if (csc.Any(xd => xd.Severity == DiagnosticSeverity.Error))
            {
                var sb = new StringBuilder();
                foreach (var xd in csc)
                {
                    var ls = xd.Location.GetLineSpan();
                    sb.AppendLine($"Error at {ls.StartLinePosition.Line:#,##0}, {ls.StartLinePosition.Character:#,##0}: {xd.GetMessage()}");
                }

                return this.Content(sb.ToString(), "text/plain", Utilities.UTF8);
            }

            Exception rex = null;
            ScriptState<object> css = null;
            var sw2 = Stopwatch.StartNew();
            try
            {
                css = await cs.RunAsync(globals).ConfigureAwait(false);
                rex = css.Exception;
            }
            catch (Exception ex)
            {
                rex = ex;
            }
            sw2.Stop();
            this.Response.Headers.Add("X-Execution-Time", sw2.ElapsedMilliseconds.ToString("#,##0"));

            if (rex != null)
                return this.Content($"Execution failed with exception\n{rex.GetType()}: {rex.Message}\n{rex.StackTrace}", "text/plain", Utilities.UTF8);

            if (css.ReturnValue != null)
            {
                this.Response.Headers.Add("X-Result-Type", css.ReturnValue.GetType().ToString());
                return this.Content(css.ReturnValue.ToString(), "text/plain", Utilities.UTF8);
            }

            return this.NoContent();
        }

        public sealed class EvaluationEnvironment
        {
            public HttpContext Context { get; }
            public IUrlHelper Url { get; }
            public SlimGetContext Database { get; }
            public RedisService Redis { get; }
            public IFileSystemService Filesystem { get; }
            public TokenService Tokens { get; }

            public EvaluationEnvironment(HttpContext ctx, IUrlHelper url, SlimGetContext db, RedisService redis, IFileSystemService fs, TokenService tokens)
            {
                this.Context = ctx;
                this.Url = url;
                this.Database = db;
                this.Redis = redis;
                this.Filesystem = fs;
                this.Tokens = tokens;
            }
        }
    }
}
