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

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SlimGet.Models;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [SlimGetRoute(Routing.MiscApiRouteName), AllowAnonymous]
    public class MiscController : Controller
    {
        private IHostingEnvironment Environment { get; }
        private SlimGetContext Database { get; }
        private TokenService Tokens { get; }

        public MiscController(IHostingEnvironment env, SlimGetContext db, TokenService tokens)
        {
            this.Environment = env;
            this.Database = db;
            this.Tokens = tokens;
        }

        // Revokes a token, provided in case of leakage; no validation is performed except GUID parsing
        [SlimGetRoute(Routing.MiscRevokeTokenRouteName), HttpGet]
        public async Task<IActionResult> RevokeToken(string token)
        {
            if (!this.Tokens.TryReadTokenId(token, out var guid))
                return this.NotFound();

            var tok = this.Database.Tokens.FirstOrDefault(x => x.Guid == guid);
            if (tok == null)
                return this.NotFound();

            this.Database.Tokens.Remove(tok);
            await this.Database.SaveChangesAsync().ConfigureAwait(false);
            return this.NoContent();
        }

        [SlimGetRoute(Routing.MiscErrorRouteName), HttpGet, ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var jsonopts = new JsonSerializerSettings { Formatting = Formatting.Indented };

            this.Response.StatusCode = 500;
            if (!this.Environment.IsDevelopment())
                return this.Json(new SimpleErrorModel(Activity.Current?.Id ?? this.HttpContext.TraceIdentifier), jsonopts);

            var exHandler = this.HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            return this.Json(new DeveloperErrorModel(Activity.Current?.Id ?? this.HttpContext.TraceIdentifier, exHandler?.Path, exHandler?.Error), jsonopts);
        }
    }
}
