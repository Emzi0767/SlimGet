using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [Route("/api/v2/package"), ApiController, Authorize]
    public sealed class PackagePublishController : NuGetControllerBase
    {
        public PackagePublishController(SlimGetContext db, RedisService redis, IFileSystemService fs, IOptions<StorageConfiguration> storcfg)
            : base(db, redis, fs, storcfg)
        { }

        [Route(""), HttpPut]
        public async Task<IActionResult> Push(CancellationToken cancellationToken)
        {
            if (this.PackageStorageConfiguration.ReadOnlyFeed)
                return this.Unauthorized();

            // spec says multipart/form-data, application/x-www-form-urlencoded should work for this too
            if (!this.Request.HasFormContentType || this.Request.Form.Files.Count <= 0)
                return this.BadRequest();

            using (var tmp = this.FileSystem.CreateTemporaryFile())
            {
                await this.Request.Form.Files.First().CopyToAsync(tmp, cancellationToken).ConfigureAwait(false);
                tmp.Position = 0;


            }

            return this.NoContent();
        }

        [Route("{id}/{version}"), HttpDelete]
        public async Task<IActionResult> Delete(string id, string version, CancellationToken cancellationToken)
            => this.NoContent();

        [Route("{id}/{version}"), HttpPost]
        public async Task<IActionResult> Relist(string id, string version, CancellationToken cancellationToken)
            => this.NoContent();
    }
}
