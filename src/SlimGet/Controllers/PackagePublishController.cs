using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SlimGet.Data;
using SlimGet.Data.Configuration;
using SlimGet.Filters;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [Route("/api/v2/package"), ApiController, Authorize, ServiceFilter(typeof(RequireWritableFeed))]
    public sealed class PackagePublishController : NuGetControllerBase
    {
        private PackageProcessingService Packages { get; }

        public PackagePublishController(SlimGetContext db, RedisService redis, IFileSystemService fs, IOptions<StorageConfiguration> storcfg, PackageProcessingService pkgParser)
            : base(db, redis, fs, storcfg)
        {
            this.Packages = pkgParser;
        }

        [Route(""), HttpPut]
        public async Task<IActionResult> Push(CancellationToken cancellationToken)
        {
            // spec says multipart/form-data, application/x-www-form-urlencoded should work for this too
            if (!this.Request.HasFormContentType || this.Request.Form.Files.Count <= 0)
                return this.BadRequest();

            var pushfile = this.Request.Form.Files.First();
            if (pushfile.Length > this.PackageStorageConfiguration.MaxPackageSizeBytes)
                return this.StatusCode(413, new { message = "Package exceeds maximum configured package size." });

            using (var pkgtmp = this.FileSystem.CreateTemporaryFile(TemporaryFileExtension.Nupkg))
            using (var spectmp = this.FileSystem.CreateTemporaryFile(TemporaryFileExtension.Nuspec))
            {
                using (var pushpkg = pushfile.OpenReadStream())
                    await pushpkg.CopyToAsync(pkgtmp, cancellationToken).ConfigureAwait(false);

                pkgtmp.Position = 0;
                var pkgparse = await this.Packages.ParsePackageAsync(pkgtmp, spectmp, cancellationToken).ConfigureAwait(false);
                if (pkgparse == null)
                    return this.BadRequest(new { message = "Package was malformed." });

                pkgtmp.Position = 0;
                spectmp.Position = 0;

                var result = await this.Packages.RegisterPackageAsync(pkgparse, this.Database, this.HttpContext.User.Identity.Name, this.FileSystem.GetPackageFileName(pkgparse.Info),
                    this.FileSystem.GetManifestFileName(pkgparse.Info), cancellationToken).ConfigureAwait(false);

                if (result == RegisterPackageResult.OwnerMismatch)
                    return this.StatusCode(403, new { message = "You are not the owner of this package." });

                if (result == RegisterPackageResult.IdMismatch)
                    return this.BadRequest(new { message = $"Package ID mismatch (check that package ID casing is identical)." });

                if (result == RegisterPackageResult.AlreadyExists)
                    return this.Conflict(new { message = "Package with specified ID and version already exists." });

                using (var pkgfs = this.FileSystem.OpenPackageWrite(pkgparse.Info))
                    await pkgtmp.CopyToAsync(pkgfs).ConfigureAwait(false);
                using (var specfs = this.FileSystem.OpenManifestWrite(pkgparse.Info))
                    await spectmp.CopyToAsync(specfs).ConfigureAwait(false);

                var pruned = await this.Packages.PrunePackageAsync(pkgparse.Id, this.PackageStorageConfiguration.LatestVersionRetainCount, this.Database, cancellationToken).ConfigureAwait(false);
                foreach (var pp in pruned)
                    this.FileSystem.DeleteWholePackage(pp);

                var (id, version) = (pkgparse.Id, pkgparse.Version.ToNormalizedString());
                return this.Created(this.Url.AbsoluteUrl("Contents", "PackageBase", this.HttpContext, new
                {
                    id,
                    id2 = id,
                    version,
                    version2 = version
                }), new { message = "Uploaded successfully." });
            }
        }

        [Route("{id}/{version}"), HttpDelete]
        public async Task<IActionResult> Delete(string id, string version, CancellationToken cancellationToken)
        {
            var pkgvdb = this.Database.PackageVersions.Include(x => x.Package).FirstOrDefault(x => x.PackageId == id && x.Version == version);
            if (pkgvdb == null)
                return this.NotFound(new { message = "Specified package ID and version were not found." });

            if (pkgvdb.Package.OwnerId != this.HttpContext.User.Identity.Name)
                return this.StatusCode(403, new { message = "You are not the owner of this package." });

            if (this.PackageStorageConfiguration.DeleteEndpointUnlists)
            {
                pkgvdb.IsListed = false;
                this.Database.PackageVersions.Update(pkgvdb);
            }
            else
            {
                this.Database.PackageVersions.Remove(pkgvdb);
                this.FileSystem.DeleteWholePackage(new PackageInfo(pkgvdb.PackageId, pkgvdb.NuGetVersion));

                if (!this.Database.PackageVersions.Any(x => x.PackageId == pkgvdb.PackageId))
                    this.Database.Packages.Remove(pkgvdb.Package);
            }

            await this.Database.SaveChangesAsync().ConfigureAwait(false);
            return this.NoContent();
        }

        [Route("{id}/{version}"), HttpPost]
        public async Task<IActionResult> Relist(string id, string version, CancellationToken cancellationToken)
        {
            var pkgvdb = this.Database.PackageVersions.Include(x => x.Package).FirstOrDefault(x => x.PackageId == id && x.Version == version);
            if (pkgvdb == null)
                return this.NotFound(new { message = "Specified package ID and version were not found." });

            if (pkgvdb.Package.OwnerId != this.HttpContext.User.Identity.Name)
                return this.StatusCode(403, new { message = "You are not the owner of this package." });

            pkgvdb.IsListed = true;
            this.Database.PackageVersions.Update(pkgvdb);
            await this.Database.SaveChangesAsync().ConfigureAwait(false);

            return this.Ok();
        }
    }
}
