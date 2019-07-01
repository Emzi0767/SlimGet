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
    [AllowAnonymous]
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
        [Route("/token/revoke/{token}"), HttpGet]
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

        [Route("/error"), HttpGet, ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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
