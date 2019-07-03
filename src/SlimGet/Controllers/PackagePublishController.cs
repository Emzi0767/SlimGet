// This file is a part of SlimGet project.
//
// Copyright 2019 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        public PackagePublishController(SlimGetContext db, RedisService redis, IFileSystemService fs, IOptions<StorageConfiguration> storcfg, ILoggerFactory logger, PackageProcessingService pkgParser)
            : base(db, redis, fs, storcfg, logger)
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

            try
            {
                using (var pkgtmp = this.FileSystem.CreateTemporaryFile(TemporaryFileExtension.Nupkg))
                using (var spectmp = this.FileSystem.CreateTemporaryFile(TemporaryFileExtension.Nuspec))
                {
                    using (var pushpkg = pushfile.OpenReadStream())
                        await pushpkg.CopyToAsync(pkgtmp, cancellationToken).ConfigureAwait(false);

                    pkgtmp.Position = 0;
                    var pkgparse = await this.Packages.ParsePackageAsync(pkgtmp, spectmp, false, cancellationToken).ConfigureAwait(false);
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

                    if (result == RegisterPackageResult.PackageCreated)
                        await this.Redis.SetPackageDownloadCountAsync(pkgparse.Info, 0).ConfigureAwait(false);
                    await this.Redis.SetVersionDownloadCountAsync(pkgparse.Info, 0).ConfigureAwait(false);

                    using (var pkgfs = this.FileSystem.OpenPackageWrite(pkgparse.Info))
                        await pkgtmp.CopyToAsync(pkgfs).ConfigureAwait(false);
                    using (var specfs = this.FileSystem.OpenManifestWrite(pkgparse.Info))
                        await spectmp.CopyToAsync(specfs).ConfigureAwait(false);

                    var pruned = await this.Packages.PrunePackageAsync(pkgparse.Id, this.PackageStorageConfiguration.LatestVersionRetainCount, this.Database, cancellationToken).ConfigureAwait(false);
                    foreach (var pp in pruned)
                        this.FileSystem.DeleteWholePackage(pp);

                    var (id, version) = (pkgparse.Id.ToLowerInvariant(), pkgparse.Version.ToNormalizedString().ToLowerInvariant());
                    return this.Created(this.Url.AbsoluteUrl("Contents", "PackageBase", this.HttpContext, new
                    {
                        id,
                        version,
                        filename = $"{id}.{version}"
                    }), new { message = "Uploaded successfully." });
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error while registering package");
                return this.BadRequest();
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
                var pkginfo = new PackageInfo(pkgvdb.PackageId, pkgvdb.NuGetVersion);

                this.Database.PackageVersions.Remove(pkgvdb);
                await this.Redis.ClearVersionDownloadCountAsync(pkginfo).ConfigureAwait(false);
                this.FileSystem.DeleteWholePackage(new PackageInfo(pkgvdb.PackageId, pkgvdb.NuGetVersion));

                if (!this.Database.PackageVersions.Any(x => x.PackageId == pkgvdb.PackageId))
                {
                    this.Database.Packages.Remove(pkgvdb.Package);
                    await this.Redis.ClearPackageDownloadCountAsync(pkginfo).ConfigureAwait(false);
                }
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
