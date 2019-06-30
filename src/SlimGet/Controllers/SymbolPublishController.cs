using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SlimGet.Controllers
{
    [Route("/api/v2/symbolpackage"), ApiController, Authorize]
    public class SymbolPublishController : ControllerBase
    {
        [Route(""), HttpPut]
        public async Task<IActionResult> Push(CancellationToken cancellationToken)
            => this.NoContent();
    }
}
