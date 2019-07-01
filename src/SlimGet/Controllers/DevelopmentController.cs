using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlimGet.Data;
using SlimGet.Data.Database;
using SlimGet.Filters;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [Route("/dev"), ApiController, ServiceFilter(typeof(RequireDevelopmentEnvironment))]
    public class DevelopmentController : Controller
    {
        private SlimGetContext Database { get; }
        private TokenService Tokens { get; }

        public DevelopmentController(SlimGetContext db, TokenService tokens)
        {
            this.Database = db;
            this.Tokens = tokens;
        }

        // Generates a user and token for testing, unless one exists already
        [Route("token/issue/{username?}/{email?}"), HttpGet]
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

        [Route("whoami"), HttpGet, Authorize(AuthenticationSchemes = TokenAuthenticationHandler.AuthenticationSchemeName)]
        public IActionResult Whoami()
            => this.Json(this.HttpContext.User.Claims.ToDictionary(x => x.Type, x => x.Value));

        [Route("genroute/{rc?}/{ra?}"), HttpGet, ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Test(string rc = "Feed", string ra = "Index")
            => this.Content(this.Url.AbsoluteUrl(ra, rc, this.HttpContext), "text/plain", Utilities.UTF8);
    }
}
