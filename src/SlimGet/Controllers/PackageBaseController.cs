using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SlimGet.Controllers
{
    [Route("/api/v3/flatcontainer"), ApiController, AllowAnonymous]
    public class PackageBaseController : ControllerBase
    {
        [Route(""), HttpGet]
        public IActionResult Dummy() => this.NoContent();

        [Route("{id}/index.json"), HttpGet]
        public async Task<IActionResult> EnumerateVersions(string id, CancellationToken cancellationToken)
            => this.NoContent();

        [Route("{id}/{version}/{id2}.{version2}.nupkg"), HttpGet]
        public async Task<IActionResult> Contents(string id, string version, string id2, string version2, CancellationToken cancellationToken)
            => this.NoContent();

        [Route("{id}/{version}/{id2}.nupkg"), HttpGet]
        public async Task<IActionResult> Manifest(string id, string version, string id2, CancellationToken cancellationToken)
            => this.NoContent();
    }
}
