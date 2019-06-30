using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SlimGet.Controllers
{
    [ApiController, AllowAnonymous]
    public class SearchController : ControllerBase
    {
        [Route("/api/v3/query"), HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery(Name = "q")] string query,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] bool prerelease = false,
            [FromQuery] string semVerLevel = null,
            CancellationToken cancellationToken = default)
            => this.NoContent();

        [Route("/api/v3/autocomplete"), HttpGet]
        public async Task<IActionResult> Autocomplete(
            [FromQuery(Name = "q")] string query,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] bool prerelease = false,
            [FromQuery] string semVerLevel = null,
            CancellationToken cancellationToken = default)
            => this.NoContent();

        [Route("/api/v3/autocomplete"), HttpGet]
        public async Task<IActionResult> Enumerate(
            [FromQuery] string id,
            [FromQuery] bool prerelease = false,
            [FromQuery] string semVerLevel = null,
            CancellationToken cancellationToken = default)
            => this.NoContent();
    }
}
