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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Frameworks;
using NuGet.Versioning;
using SlimGet.Data.Configuration;
using SlimGet.Data.Database;
using SlimGet.Models;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [SlimGetRoute(Routing.GalleryRouteName), AllowAnonymous]
    public class GalleryController : Controller
    {
        private PackageStorageConfiguration PackageStorageConfiguration { get; }
        private SlimGetContext Database { get; }

        public GalleryController(IOptions<StorageConfiguration> scfg, SlimGetContext db)
        {
            this.PackageStorageConfiguration = scfg.Value.Packages;
            this.Database = db;
        }

        [HttpGet, SlimGetRoute(Routing.GalleryIndexRouteName), SlimGetRoute(Routing.InheritRoute)]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pkgCount = await this.Database.Packages.CountAsync(cancellationToken).ConfigureAwait(false);
            var verCount = await this.Database.PackageVersions.CountAsync(cancellationToken).ConfigureAwait(false);

            return this.View(new GalleryIndexModel(pkgCount, verCount));
        }

        [HttpGet, SlimGetRoute(Routing.GalleryListRouteName)]
        public async Task<IActionResult> Packages([FromQuery] int skip, CancellationToken cancellationToken)
        {
            if (skip < 0)
                return this.BadRequest();

            var dbpackages = this.Database.Packages
                .Include(x => x.Tags)
                .Include(x => x.Authors)
                .Include(x => x.Versions)
                .OrderByDescending(x => x.DownloadCount)
                .ThenBy(x => x.Id);

            var count = await dbpackages.CountAsync(cancellationToken);
            var next = skip + 20 <= count ? skip + 20 : -1;

            return this.View("Packages", new GallerySearchListModel(count, this.PreparePackages(dbpackages.Skip(skip).Take(20)), next, skip - 20, null));
        }

        [HttpGet, SlimGetRoute(Routing.GalleryPackageRouteName)]
        public async Task<IActionResult> Package(string id, string version, CancellationToken cancellationToken)
        {
            var dbpackage = await this.Database.Packages
                .Include(x => x.Tags)
                .Include(x => x.Versions)
                .ThenInclude(x => x.Binaries)
                .ThenInclude(x => x.PackageSymbols)
                .Include(x => x.Versions)
                .ThenInclude(x => x.Dependencies)
                .Include(x => x.Authors)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);

            if (dbpackage == null)
                return this.NotFound();

            var dbversion = version != null
                ? dbpackage.Versions.FirstOrDefault(x => x.Version == version)
                : dbpackage.Versions.OrderByDescending(x => x.NuGetVersion).First();

            if (dbversion == null)
                return this.NotFound();

            return this.View(this.PreparePackage(dbpackage, dbversion));
        }

        [HttpGet, SlimGetRoute(Routing.GallerySearchRouteName)]
        public async Task<IActionResult> Search([FromQuery] GallerySearchModel search, CancellationToken cancellationToken)
        {
            var query = search.Query;
            var prerelease = search.Prerelease;
            var skip = search.Skip;

            IQueryable<Package> dbpackages = this.Database.Packages
                    .Include(x => x.Versions)
                    .Include(x => x.Tags)
                    .Include(x => x.Authors);

            if (!string.IsNullOrWhiteSpace(search.Query))
                dbpackages = dbpackages.Where(x => (EF.Functions.Similarity(x.Id, query) >= 0.35 ||
                        EF.Functions.Similarity(x.Description, query) >= 0.2 ||
                        EF.Functions.Similarity(x.Title, query) >= 0.2 ||
                        x.Tags.Any(y => EF.Functions.Similarity(y.Tag, query) >= 0.35)) &&
                    x.Versions.Any(y => (!y.IsPrerelase || prerelease) && y.IsListed));

            else
                dbpackages = dbpackages.Where(x => x.Versions.Any(y => (!y.IsPrerelase || prerelease) && y.IsListed));

            dbpackages = dbpackages
                .OrderByDescending(x => x.DownloadCount)
                .ThenBy(x => x.Id);

            var count = await dbpackages.CountAsync(cancellationToken);
            var next = skip + 20 <= count ? skip + 20 : -1;

            return this.View("Packages", new GallerySearchListModel(count, this.PreparePackages(dbpackages.Skip(skip).Take(20), prerelease), next, skip - 20, search));
        }

        [HttpGet, SlimGetRoute(Routing.GalleryAboutRouteName)]
        public IActionResult About()
        {
            var feedUrl = this.Url.AbsoluteUrl(Routing.FeedIndexRouteName, this.HttpContext);
            var symbolUrl = this.Url.AbsoluteUrl(Routing.DownloadSymbolsRouteName, this.HttpContext);
            var symbolPushUrl = this.Url.AbsoluteUrl(Routing.PublishSymbolsRouteName, this.HttpContext);

            return this.View(new GalleryAboutModel(
                new Uri(feedUrl),
                new Uri(symbolUrl),
                new Uri(symbolPushUrl),
                this.PackageStorageConfiguration.SymbolsEnabled,
                !this.PackageStorageConfiguration.ReadOnlyFeed));
        }

        private IEnumerable<GalleryPackageListItemModel> PreparePackages(IEnumerable<Package> dbpackages, bool prerelease = true)
        {
            foreach (var dbpackage in dbpackages)
            {
                var version = dbpackage.Versions
                    .Where(x => !x.IsPrerelase || prerelease)
                    .OrderByDescending(x => x.NuGetVersion)
                    .First();

                yield return new GalleryPackageListItemModel
                {
                    Id = dbpackage.Id,
                    Title = dbpackage.Title,
                    IconUrl = dbpackage.IconUrl,
                    Authors = dbpackage.AuthorNames,
                    Tags = dbpackage.TagNames,
                    DownloadCount = dbpackage.DownloadCount,
                    PublishedAt = dbpackage.PublishedAt.Value,
                    LastUpdatedAt = version.PublishedAt.Value,
                    LatestVersion = version.NuGetVersion,
                    Description = dbpackage.Description
                };
            }
        }

        private GalleryPackageInfoModel PreparePackage(Package dbpackage, PackageVersion dbversion)
            => new GalleryPackageInfoModel
            {
                Id = dbpackage.Id,
                Title = dbpackage.Title,
                IconUrl = dbpackage.IconUrl,
                ProjectUrl = dbpackage.ProjectUrl,
                LicenseUrl = dbpackage.LicenseUrl,
                RepositoryUrl = dbpackage.RepositoryUrl,
                Authors = dbpackage.AuthorNames,
                Tags = dbpackage.TagNames,
                DownloadCount = dbpackage.DownloadCount,
                VersionDownloadCount = dbversion.DownloadCount,
                PublishedAt = dbversion.PublishedAt.Value,
                Version = dbversion.NuGetVersion,
                Description = dbpackage.Description,
                DownloadUrl = this.Url.AbsoluteUrl(Routing.DownloadPackageContentsRouteName, this.HttpContext, new
                {
                    id = dbpackage.IdLowercase,
                    version = dbversion.VersionLowercase,
                    filename = $"{dbpackage.IdLowercase}.{dbversion.VersionLowercase}"
                }),
                ManifestUrl = this.Url.AbsoluteUrl(Routing.DownloadPackageManifestRouteName, this.HttpContext, new
                {
                    id = dbpackage.IdLowercase,
                    version = dbversion.VersionLowercase,
                    id2 = dbpackage.IdLowercase
                }),
                // figure out symbols
                DependencyGroups = this.PrepareDependencyGroups(dbversion),
                OwnerId = dbpackage.OwnerId,
                AllVersions = dbpackage.Versions.Select(x => (x.Version, x.DownloadCount, new DateTimeOffset(x.PublishedAt.Value)))
            };

        private IEnumerable<GalleryPackageDependencyGroupModel> PrepareDependencyGroups(PackageVersion dbversion)
        {
            foreach (var depgroup in dbversion.Dependencies.GroupBy(x => x.TargetFramework))
                yield return new GalleryPackageDependencyGroupModel
                {
                    Framework = NuGetFramework.Parse(depgroup.Key),
                    Dependencies = this.PrepareDependencies(depgroup)
                };
        }

        private IEnumerable<GalleryPackageDependencyModel> PrepareDependencies(IEnumerable<PackageDependency> dbdeps)
        {
            foreach (var dbdep in dbdeps)
                yield return new GalleryPackageDependencyModel
                {
                    Id = dbdep.Id,
                    MinVersion = dbdep.MinVersion != null ? NuGetVersion.Parse(dbdep.MinVersion) : null,
                    MaxVersion = dbdep.MaxVersion != null ? NuGetVersion.Parse(dbdep.MaxVersion) : null,
                    MinInclusive = dbdep.IsMinVersionInclusive.HasValue && dbdep.IsMinVersionInclusive.Value,
                    MaxInclusive = dbdep.IsMaxVersionInclusive.HasValue && dbdep.IsMaxVersionInclusive.Value
                };
        }
    }
}
