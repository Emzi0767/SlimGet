using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SlimGet.Controllers
{
    [Route("/api/v3/registration"), ApiController, AllowAnonymous]
    public class RegistrationsBaseController : ControllerBase
    {
        [Route("plain"), HttpGet]
        public IActionResult Dummy() => this.NoContent();

        [Route("gzip"), HttpGet]
        public IActionResult DummyGz() => this.NoContent();

        [Route("semver2"), HttpGet]
        public IActionResult DummySemVer() => this.NoContent();

        [Route("{mode}/{id}/index.json"), HttpGet]
        public async Task<IActionResult> Index(string id, RegistrationsContentMode mode, CancellationToken cancellationToken)
            => this.NoContent();

        [Route("{mode}/{id}/page/{lowestVersion}/{highestVersion}.json"), HttpGet]
        public async Task<IActionResult> Page(string id, string lowestVersion, string highestVersion, CancellationToken cancellationToken)
            => this.NoContent();

        [Route("{mode}/{id}/{version}.json"), HttpGet]
        public async Task<IActionResult> Leaf(string id, string version, RegistrationsContentMode mode, CancellationToken cancellationToken)
            => this.NoContent();
    }

    public enum RegistrationsContentMode
    {
        Plain,
        GZip,
        SemVer2
    }
}
