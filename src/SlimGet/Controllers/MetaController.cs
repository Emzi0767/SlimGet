using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SlimGet.Data;
using SlimGet.Data.Database;
using SlimGet.Models;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [AllowAnonymous]
    public class MetaController : Controller
    {
        private IHostingEnvironment Environment { get; }
        private SlimGetContext Database { get; }
        private TokenService Tokens { get; }

        public MetaController(IHostingEnvironment env, SlimGetContext db, TokenService tokens)
        {
            this.Environment = env;
            this.Database = db;
            this.Tokens = tokens;
        }

        // Generates a user and token for testing, unless one exists already
        [Route("/token/issue/{username?}/{email?}"), HttpGet]
        public async Task<IActionResult> Token(string username = null, string email = null)
        {
            if (!this.Environment.IsDevelopment())
                return this.Unauthorized();

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

        [Route("/whoami")]
        public IActionResult Whoami()
        {
            if (!this.Environment.IsDevelopment() || !this.HttpContext.User.Identity.IsAuthenticated)
                return this.Unauthorized();

            var claims = this.HttpContext.User.Claims.ToDictionary(x => x.Type, x => x.Value);
            return this.Json(claims);
        }

        [Route("/test/{rc?}/{ra?}"), HttpGet, ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Test(string rc = "Feed", string ra = "Index")
        {
            return this.Environment.IsDevelopment()
                ? this.Content(this.Url.AbsoluteUrl(ra, rc, this.HttpContext), "text/plain", Utilities.UTF8)
                : this.Unauthorized() as IActionResult;
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
