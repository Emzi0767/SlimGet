using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SlimGet.Data;
using SlimGet.Data.Configuration;
using SlimGet.Models;
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
        {
            var pkg = await this.Database.Packages.Include(x => x.Versions)
                .FirstOrDefaultAsync(x => x.IdLowercase == id, cancellationToken);

            if (pkg == null)
                return this.NotFound();

            return this.Json(new PackageVersionList(pkg.Versions.Select(x => x.Version)));
        }

        [Route("{id}/{version}/{filename}.nupkg"), HttpGet]
        public async Task<IActionResult> Contents(string id, string version, string filename, CancellationToken cancellationToken)
        {
            if (filename != $"{id}.{version}")
                return this.NotFound();

            var pkg = await this.Database.Packages.Include(x => x.Versions)
                .FirstOrDefaultAsync(x => x.IdLowercase == id, cancellationToken);
            if (pkg == null)
                return this.NotFound();

            var pkgv = pkg.Versions.FirstOrDefault(x => x.VersionLowercase == version);
            if (pkgv == null)
                return this.NotFound();

#warning Increment counters
            var pkginfo = new PackageInfo(pkg.Id, pkgv.NuGetVersion);
            var pkgdata = this.FileSystem.OpenPackageRead(pkginfo);
            return this.File(pkgdata, "application/octet-stream", $"{pkg.Id}.{pkgv.Version}.nupkg");
        }

        [Route("{id}/{version}/{id2}.nuspec"), HttpGet]
        public async Task<IActionResult> Manifest(string id, string version, string id2, CancellationToken cancellationToken)
        {
            if (id != id2)
                return this.NotFound();

            var pkg = await this.Database.Packages.Include(x => x.Versions)
                .FirstOrDefaultAsync(x => x.IdLowercase == id, cancellationToken);
            if (pkg == null)
                return this.NotFound();

            var pkgv = pkg.Versions.FirstOrDefault(x => x.VersionLowercase == version);
            if (pkgv == null)
                return this.NotFound();

#warning Increment counters
            var pkginfo = new PackageInfo(pkg.Id, pkgv.NuGetVersion);
            var pkgdata = this.FileSystem.OpenManifestRead(pkginfo);
            return this.File(pkgdata, "application/xml", $"{pkg.Id}.nuspec");
        }
    }
}
