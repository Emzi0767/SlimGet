using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [Route("/api/v3/flatcontainer"), ApiController, AllowAnonymous]
    public class PackageBaseController : NuGetControllerBase
    {
        public PackageBaseController(SlimGetContext db, RedisService redis, IFileSystemService fs, IOptions<StorageConfiguration> storcfg)
            : base(db, redis, fs, storcfg)
        { }

        [Route(""), HttpGet]
        public IActionResult Dummy() => this.NoContent();

        [Route("{id}/index.json"), HttpGet]
        public async Task<IActionResult> EnumerateVersions(string id, CancellationToken cancellationToken)
            => this.NoContent();

        [Route("{id}/{version}/{id2}.{version2}.nupkg"), HttpGet]
        public async Task<IActionResult> Contents(string id, string version, string id2, string version2, CancellationToken cancellationToken)
            => this.NoContent();

        [Route("{id}/{version}/{id2}.nuspec"), HttpGet]
        public async Task<IActionResult> Manifest(string id, string version, string id2, CancellationToken cancellationToken)
            => this.NoContent();
    }
}
