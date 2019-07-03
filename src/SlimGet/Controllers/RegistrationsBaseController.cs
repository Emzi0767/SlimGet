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

using System.IO.Compression;
using System.Linq;
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
using SlimGet.Models;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [Route("/api/v3/registration"), ApiController, AllowAnonymous]
    public class RegistrationsBaseController : NuGetControllerBase
    {
        public RegistrationsBaseController(SlimGetContext db, RedisService redis, IFileSystemService fs, IOptions<StorageConfiguration> storcfg, ILoggerFactory logger)
            : base(db, redis, fs, storcfg, logger)
        { }

        [Route("plain"), HttpGet]
        public IActionResult Dummy() => this.NoContent();

        [Route("gzip"), HttpGet]
        public IActionResult DummyGz() => this.NoContent();

        [Route("semver2"), HttpGet]
        public IActionResult DummySemVer() => this.NoContent();

        [Route("{mode}/{id}/index.json"), HttpGet]
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
                .FirstOrDefaultAsync(x => x.IdLowercase == id, cancellationToken)
                .ConfigureAwait(false);
            if (dbpackage == null)
                return this.NotFound();

            var dbversions = dbpackage.Versions
                .Where(x => !useSemVer2 ? !x.NuGetVersion.IsSemVer2 : true)
                .OrderBy(x => x.NuGetVersion)
                .ToList();
            var inline = dbversions.Count <= 128;
            var pageCount = dbversions.Count / 64 + (dbversions.Count % 64 != 0 ? 1 : 0);
            var pages = Enumerable.Range(0, pageCount)
                .Select(x => dbversions.Skip(x * 64).Take(64));

            return this.NoContent();
        }

        [Route("{mode}/{id}/page/{minVersion}/{maxVersion}.json"), HttpGet]
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
                .FirstOrDefaultAsync(x => x.IdLowercase == id, cancellationToken)
                .ConfigureAwait(false);
            if (dbpackage == null)
                return this.NotFound();

            var nvMin = NuGetVersion.Parse(minVersion);
            var nvMax = NuGetVersion.Parse(maxVersion);
            var dbversions = dbpackage.Versions
                .Where(x => x.NuGetVersionLowercase >= nvMin && x.NuGetVersionLowercase <= nvMax && (!useSemVer2 ? !x.NuGetVersion.IsSemVer2 : true))
                .ToList();
            if (dbversions.Count > 64)
                return this.BadRequest(new { message = "Too wide version range." });

            return this.Json(new RegistrationsPageModel
            {
                PageUrl = this.Url.AbsoluteUrl("Page", "RegistrationsBase", this.HttpContext, new { mode, id, minVersion, maxVersion }),
                Count = dbversions.Count,
                Items = dbversions.Select(x => new RegistrationsLeafModel
                {
                    LeafUrl = this.Url.AbsoluteUrl("Leaf", "RegistrationsBase", this.HttpContext, new { mode, id, version = x.VersionLowercase }),
                    IsListed = x.IsListed,
                    ContentUrl = this.Url.AbsoluteUrl("Contents", "PackageBase", this.HttpContext, new { id, version = x.VersionLowercase, filename = $"{id}.{x.VersionLowercase}" }),
                    PublishedAt = x.PublishedAt.Value,
                    RegistrationIndexUrl = this.Url.AbsoluteUrl("Index", "RegistrationsBase", this.HttpContext, new { mode, id })
                }),
                MinVersion = dbversions.Min(x => x.NuGetVersion).ToNormalizedString(),
                MaxVersion = dbversions.Max(x => x.NuGetVersion).ToNormalizedString(),
                IndexUrl = this.Url.AbsoluteUrl("Index", "RegistrationsBase", this.HttpContext, new { mode, id })
            });
        }

        [Route("{mode}/{id}/{version}.json"), HttpGet]
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
                .FirstOrDefaultAsync(x => x.IdLowercase == id, cancellationToken)
                .ConfigureAwait(false);
            if (dbpackage == null)
                return this.NotFound();

            var dbversion = dbpackage.Versions.FirstOrDefault(x => x.VersionLowercase == version && (!useSemVer2 ? !x.NuGetVersion.IsSemVer2 : true));
            if (dbversion == null)
                return this.NotFound();

            return this.Json(new RegistrationsLeafModel
            {
                LeafUrl = this.Url.AbsoluteUrl("Leaf", "RegistrationsBase", this.HttpContext, new { mode, id, version }),
                IsListed = dbversion.IsListed,
                ContentUrl = this.Url.AbsoluteUrl("Contents", "PackageBase", this.HttpContext, new { id, version, filename = $"{id}.{version}" }),
                PublishedAt = dbversion.PublishedAt.Value,
                RegistrationIndexUrl = this.Url.AbsoluteUrl("Index", "RegistrationsBase", this.HttpContext, new { mode, id })
            });
        }
    }

    public enum RegistrationsContentMode
    {
        Plain,
        GZip,
        SemVer2
    }
}
