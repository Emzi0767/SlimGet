using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [Route("/api/v2/symbolpackage"), ApiController, Authorize]
    public class SymbolPublishController : NuGetControllerBase
    {
        public SymbolPublishController(SlimGetContext db, RedisService redis, IOptions<StorageConfiguration> storcfg)
            : base(db, redis, storcfg)
        { }

        [Route(""), HttpPut]
        public async Task<IActionResult> Push(CancellationToken cancellationToken)
            => this.NoContent();
    }
}
