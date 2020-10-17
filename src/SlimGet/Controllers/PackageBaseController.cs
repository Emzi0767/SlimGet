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
using SlimGet.Models;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [SlimGetRoute(Routing.DownloadPackageRouteName), ApiController, AllowAnonymous]
    public class PackageBaseController : NuGetControllerBase
    {
        public PackageBaseController(
            SlimGetContext db,
            RedisService redis,
            IFileSystemService fs,
            IOptions<BlobStorageConfiguration> blobstoreOpts,
            IOptions<PackageStorageConfiguration> packageOpts,
            ILoggerFactory logger)
            : base(db, redis, fs, blobstoreOpts, packageOpts, logger)
        { }

        [SlimGetRoute(Routing.InheritRoute), HttpGet]
        public IActionResult Dummy()
            => this.NotFound();

        [SlimGetRoute(Routing.DownloadPackageIndexRouteName), HttpGet]
        public async Task<IActionResult> EnumerateVersions(string id, CancellationToken cancellationToken)
        {
            var pkg = await this.Database.Packages.Include(x => x.Versions)
                .FirstOrDefaultAsync(x => x.IdLowercase == id, cancellationToken);

            if (pkg == null)
                return this.NotFound();

            return this.Json(new PackageVersionList(pkg.Versions.Select(x => x.Version)));
        }

        [SlimGetRoute(Routing.DownloadPackageContentsRouteName), HttpGet]
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

            var pkginfo = new PackageInfo(pkg.Id, pkgv.NuGetVersion);
            await this.Redis.IncrementPackageDownloadCountAsync(pkginfo).ConfigureAwait(false);
            await this.Redis.IncrementVersionDownloadCountAsync(pkginfo).ConfigureAwait(false);

            var pkgdata = this.FileSystem.OpenPackageRead(pkginfo);
            return this.File(pkgdata, "application/octet-stream", $"{pkg.Id}.{pkgv.Version}.nupkg");
        }

        [SlimGetRoute(Routing.DownloadPackageManifestRouteName), HttpGet]
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

            var pkginfo = new PackageInfo(pkg.Id, pkgv.NuGetVersion);
            var pkgdata = this.FileSystem.OpenManifestRead(pkginfo);
            return this.File(pkgdata, "application/xml", $"{pkg.Id}.nuspec");
        }
    }
}
