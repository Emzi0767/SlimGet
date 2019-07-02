using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;
using SlimGet.Models;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [ApiController, AllowAnonymous]
    public class SearchController : NuGetControllerBase
    {
        public SearchController(SlimGetContext db, RedisService redis, IFileSystemService fs, IOptions<StorageConfiguration> storcfg, ILoggerFactory logger)
            : base(db, redis, fs, storcfg, logger)
        { }

        [Route("/api/v3/query"), HttpGet]
        public async Task<IActionResult> Search(SearchQueryModel search, CancellationToken cancellationToken)
            => this.NoContent();

        [Route("/api/v3/autocomplete"), HttpGet]
        public async Task<IActionResult> Autocomplete(SearchQueryModel search, CancellationToken cancellationToken)
            => this.NoContent();

        [Route("/api/v3/autocomplete"), HttpGet]
        public async Task<IActionResult> Enumerate(SearchEnumerateModel enumerate, CancellationToken cancellationToken)
            => this.NoContent();
    }
}
