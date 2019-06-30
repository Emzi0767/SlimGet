using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SlimGet.Controllers
{
    [Route("/api/v2/package"), ApiController, Authorize]
    public sealed class PackagePublishController : ControllerBase
    {
        [Route(""), HttpPut]
        public async Task<IActionResult> Push(CancellationToken cancellationToken)
            => this.NoContent();

        [Route("{id}/{version}"), HttpDelete]
        public async Task<IActionResult> Delete(string id, string version, CancellationToken cancellationToken)
            => this.NoContent();

        [Route("{id}/{version}"), HttpPost]
        public async Task<IActionResult> Relist(string id, string version, CancellationToken cancellationToken)
            => this.NoContent();
    }
}
