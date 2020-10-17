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

using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NuGet.Versioning;
using SlimGet.Data.Configuration;
using SlimGet.Data.Database;
using SlimGet.Models;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [SlimGetRoute(Routing.RegistrationsRouteName), ApiController, AllowAnonymous]
    public class RegistrationsBaseController : NuGetControllerBase
    {
        public RegistrationsBaseController(
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

        [SlimGetRoute(Routing.RegistrationsIndexRouteName), HttpGet]
        public async Task<IActionResult> Index(string id, RegistrationsContentMode mode, CancellationToken cancellationToken)
        {
            var useGz = mode == RegistrationsContentMode.GZip || mode == RegistrationsContentMode.SemVer2;
            var useSemVer2 = mode == RegistrationsContentMode.SemVer2;
            if (useGz)
            {
                this.Response.Headers.Add(HeaderNames.ContentEncoding, "gzip");
                this.Response.Body = new GZipStream(this.Response.Body, CompressionLevel.Optimal);
            }

            var dbpackage = await this.Database.Packages
                .Include(x => x.Versions)
                .ThenInclude(x => x.Dependencies)
                .Include(x => x.Authors)
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(x => x.IdLowercase == id, cancellationToken)
                .ConfigureAwait(false);
            if (dbpackage == null)
                return this.NotFound();

            var dbversions = dbpackage.Versions
                .Where(x => !useSemVer2 ? !x.NuGetVersion.IsSemVer2 : true)
                .OrderBy(x => x.NuGetVersion)
                .ToList();

            return this.Json(this.PrepareIndex(mode, dbpackage, dbversions));
        }

        [SlimGetRoute(Routing.RegistrationsPageRouteName), HttpGet]
        public async Task<IActionResult> Page(string id, string minVersion, string maxVersion, RegistrationsContentMode mode, CancellationToken cancellationToken)
        {
            var useGz = mode == RegistrationsContentMode.GZip || mode == RegistrationsContentMode.SemVer2;
            var useSemVer2 = mode == RegistrationsContentMode.SemVer2;
            if (useGz)
            {
                this.Response.Headers.Add(HeaderNames.ContentEncoding, "gzip");
                this.Response.Body = new GZipStream(this.Response.Body, CompressionLevel.Optimal);
            }

            var dbpackage = await this.Database.Packages
                .Include(x => x.Versions)
                .ThenInclude(x => x.Dependencies)
                .Include(x => x.Authors)
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(x => x.IdLowercase == id, cancellationToken)
                .ConfigureAwait(false);
            if (dbpackage == null)
                return this.NotFound();

            var nvMin = NuGetVersion.Parse(minVersion);
            var nvMax = NuGetVersion.Parse(maxVersion);
            var dbversions = dbpackage.Versions
                .Where(x => x.NuGetVersionLowercase >= nvMin && x.NuGetVersionLowercase <= nvMax && (!useSemVer2 ? !x.NuGetVersion.IsSemVer2 : true))
                .OrderBy(x => x.NuGetVersion)
                .ToList();
            if (dbversions.Count > 64)
                return this.BadRequest(new { message = "Too wide version range." });

            return this.Json(this.PreparePage(mode, dbpackage, dbversions, (minVersion, maxVersion), true));
        }

        [SlimGetRoute(Routing.RegistrationsLeafRouteName), HttpGet]
        public async Task<IActionResult> Leaf(string id, string version, RegistrationsContentMode mode, CancellationToken cancellationToken)
        {
            var useGz = mode == RegistrationsContentMode.GZip || mode == RegistrationsContentMode.SemVer2;
            var useSemVer2 = mode == RegistrationsContentMode.SemVer2;
            if (useGz)
            {
                this.Response.Headers.Add(HeaderNames.ContentEncoding, "gzip");
                this.Response.Body = new GZipStream(this.Response.Body, CompressionLevel.Optimal);
            }

            var dbpackage = await this.Database.Packages
                .Include(x => x.Versions)
                .ThenInclude(x => x.Dependencies)
                .Include(x => x.Authors)
                .Include(x => x.Tags)
                .FirstOrDefaultAsync(x => x.IdLowercase == id, cancellationToken)
                .ConfigureAwait(false);
            if (dbpackage == null)
                return this.NotFound();

            var dbversion = dbpackage.Versions.FirstOrDefault(x => x.VersionLowercase == version && (!useSemVer2 ? !x.NuGetVersion.IsSemVer2 : true));
            if (dbversion == null)
                return this.NotFound();

            return this.Json(this.PrepareLeaf(mode, dbpackage, dbversion));
        }

        private RegistrationsLeafModel PrepareLeaf(RegistrationsContentMode mode, Package dbpackage, PackageVersion dbversion)
            => new RegistrationsLeafModel
            {
                LeafUrl = this.Url.AbsoluteUrl("Leaf", "RegistrationsBase", this.HttpContext, new { mode, id = dbpackage.IdLowercase, version = dbversion.VersionLowercase }),
                CatalogEntry = new CatalogEntryModel
                {
                    CatalogUrl = this.Url.AbsoluteUrl("Leaf", "RegistrationsBase", this.HttpContext, new { mode, id = dbpackage.IdLowercase, version = dbversion.VersionLowercase }),
                    Authors = dbpackage.AuthorNames,
                    DependencyGroups = this.PrepareDependencyGroups(dbversion),
                    Deprecation = null,
                    Description = dbpackage.Description,
                    IconUrl = dbpackage.IconUrl,
                    Id = dbpackage.Id,
                    LicenseUrl = dbpackage.LicenseUrl,
                    LicenseExpression = null,
                    IsListed = dbpackage.IsListed,
                    MinimumClientVersion = dbpackage.MinimumClientVersion,
                    ProjectUrl = dbpackage.ProjectUrl,
                    PublishedAt = dbpackage.PublishedAt.Value,
                    RequiresLicenseAcceptance = dbpackage.RequiresLicenseAcceptance,
                    Summary = dbpackage.Summary,
                    Tags = dbpackage.TagNames,
                    Title = dbpackage.Title,
                    Version = dbversion.Version
                },
                IsListed = dbversion.IsListed,
                ContentUrl = this.Url.AbsoluteUrl("Contents", "PackageBase", this.HttpContext, new
                {
                    id = dbpackage.IdLowercase,
                    version = dbversion.VersionLowercase,
                    filename = $"{dbpackage.IdLowercase}.{dbversion.VersionLowercase}"
                }),
                PublishedAt = dbversion.PublishedAt.Value,
                RegistrationIndexUrl = this.Url.AbsoluteUrl("Index", "RegistrationsBase", this.HttpContext, new { mode, id = dbpackage.IdLowercase })
            };

        private RegistrationsPageModel PreparePage(RegistrationsContentMode mode, Package dbpackage, IEnumerable<PackageVersion> dbversions, (string min, string max) versionRange, bool inlineVersions)
        {
            var items = !inlineVersions
                ? null
                : dbversions.Select(x => this.PrepareLeaf(mode, dbpackage, x));

            return new RegistrationsPageModel
            {
                PageUrl = this.Url.AbsoluteUrl("Page", "RegistrationsBase", this.HttpContext, new
                {
                    mode,
                    id = dbpackage.IdLowercase,
                    minVersion = versionRange.min,
                    maxVersion = versionRange.max
                }),
                Count = dbversions.Count(),
                Items = items,
                MinVersion = versionRange.min,
                MaxVersion = versionRange.max,
                IndexUrl = this.Url.AbsoluteUrl("Index", "RegistrationsBase", this.HttpContext, new { mode, id = dbpackage.IdLowercase })
            };
        }

        private RegistrationsIndexModel PrepareIndex(RegistrationsContentMode mode, Package dbpackage, IEnumerable<PackageVersion> dbversions)
        {
            var count = dbversions.Count();
            var inline = count <= 128;
            var pages = new List<RegistrationsPageModel>();
            for (var i = 0; i < count; i += 64)
            {
                var versions = dbversions.Skip(i).Take(64);

                // Versions collection should be ordered by the time it enters the method
                // Thusly we can grab minmax from the edges
                var min = versions.First().VersionLowercase;
                var max = versions.Last().VersionLowercase;

                pages.Add(this.PreparePage(mode, dbpackage, versions, (min, max), inline));
            }

            return new RegistrationsIndexModel
            {
                Count = pages.Count,
                Pages = pages
            };
        }

        private IEnumerable<DependencyGroupModel> PrepareDependencyGroups(PackageVersion dbversion)
        {
            foreach (var depgroup in dbversion.Dependencies.GroupBy(x => x.TargetFramework))
                yield return new DependencyGroupModel
                {
                    Framework = depgroup.Key,
                    Dependencies = this.PrepareDependencies(depgroup)
                };
        }

        private IEnumerable<DependencyModel> PrepareDependencies(IEnumerable<PackageDependency> dbdeps)
        {
            foreach (var dbdep in dbdeps)
                yield return new DependencyModel
                {
                    Id = dbdep.Id,
                    VersionRange = this.PrepareVersionRange(dbdep),
                    RegistrationUrl = null
                };
        }

        private string PrepareVersionRange(PackageDependency dbdep)
        {
            var sb = new StringBuilder();
            if (dbdep.MinVersion != null)
                sb.Append(dbdep.IsMinVersionInclusive == true ? '[' : '(')
                    .Append(dbdep.MinVersion);
            else
                sb.Append('(');

            sb.Append(',');

            if (dbdep.MaxVersion != null)
                sb.Append(dbdep.IsMaxVersionInclusive == true ? ']' : ')')
                    .Append(dbdep.MaxVersion);
            else
                sb.Append(')');

            return sb.ToString();
        }
    }

    public enum RegistrationsContentMode
    {
        Plain,
        GZip,
        SemVer2
    }
}
