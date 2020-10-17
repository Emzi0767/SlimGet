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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;
using SlimGet.Data.Database;
using SlimGet.Models;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [SlimGetRoute(Routing.SearchRouteName), ApiController, AllowAnonymous]
    public class SearchController : NuGetControllerBase
    {
        public SearchController(
            SlimGetContext db,
            RedisService redis,
            IFileSystemService fs,
            IOptions<BlobStorageConfiguration> blobstoreOpts,
            IOptions<PackageStorageConfiguration> packageOpts,
            ILoggerFactory logger)
            : base(db, redis, fs, blobstoreOpts, packageOpts, logger)
        { }

        [SlimGetRoute(Routing.SearchQueryRouteName), HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchQueryModel search, CancellationToken cancellationToken)
        {
            var semver2 = search.SemVerLevel == "2.0.0";
            var prerelease = search.Prerelease;
            var query = search.Query;

            IQueryable<Package> dbpackages = this.Database.Packages
                    .Include(x => x.Versions)
                    .Include(x => x.Tags)
                    .Include(x => x.Authors);

            if (!string.IsNullOrWhiteSpace(search.Query))
                dbpackages = dbpackages.Where(x => (EF.Functions.TrigramsSimilarity(x.Id, query) >= 0.35 ||
                        EF.Functions.TrigramsSimilarity(x.Description, query) >= 0.2 ||
                        EF.Functions.TrigramsSimilarity(x.Title, query) >= 0.2 ||
                        x.Tags.Any(y => EF.Functions.TrigramsSimilarity(y.Tag, query) >= 0.35)) &&
                    (x.SemVerLevel == SemVerLevel.Unknown || semver2) &&
                    x.Versions.Any(y => (!y.IsPrerelase || prerelease) && y.IsListed));

            else
                dbpackages = dbpackages.Where(x => (x.SemVerLevel == SemVerLevel.Unknown || semver2) &&
                    x.Versions.Any(y => (!y.IsPrerelase || prerelease) && y.IsListed));

            var count = await dbpackages.CountAsync(cancellationToken).ConfigureAwait(false);

            return this.Json(this.PrepareResponse(dbpackages, count, prerelease, search.Skip, search.Take));
        }

        [SlimGetRoute(Routing.SearchAutocompleteRouteName), HttpGet]
        public async Task<IActionResult> Autocomplete([FromQuery] SearchQueryModel search, CancellationToken cancellationToken)
        {
            var semver2 = search.SemVerLevel == "2.0.0";
            var prerelease = search.Prerelease;
            if (string.IsNullOrWhiteSpace(search.Id))
            {
                var query = search.Query;
                var dbids = this.Database.Packages
                    .Include(x => x.Versions)
                    .Include(x => x.Tags)
                    .Include(x => x.Authors)
                    .Where(x => (EF.Functions.TrigramsSimilarity(x.Id, query) >= 0.35 ||
                            EF.Functions.TrigramsSimilarity(x.Description, query) >= 0.2 ||
                            EF.Functions.TrigramsSimilarity(x.Title, query) >= 0.2 ||
                            x.Tags.Any(y => EF.Functions.TrigramsSimilarity(y.Tag, query) >= 0.35)) &&
                        (x.SemVerLevel == SemVerLevel.Unknown || semver2) &&
                        x.Versions.Any(y => (!y.IsPrerelase || prerelease) && y.IsListed))
                    .Select(x => x.Id);

                var count = await dbids.CountAsync(cancellationToken).ConfigureAwait(false);

                return this.Json(new SearchAutocompleteResponseModel
                {
                    TotalResultCount = count,
                    Results = dbids
                });
            }
            else
            {
                var id = search.Id;
                var dbversions = this.Database.PackageVersions
                .Include(x => x.Package)
                .Where(x => x.PackageId == id &&
                    (!x.IsPrerelase || prerelease) &&
                    (x.Package.SemVerLevel == SemVerLevel.Unknown || semver2))
                .Select(x => x.Version);

                return this.Json(new SearchEnumerateResponseModel { Versions = dbversions });
            }
        }

        public SearchResponseModel PrepareResponse(IEnumerable<Package> dbpackages, int totalCount, bool prerelease, int skip, int take)
            => new SearchResponseModel
            {
                TotalResultCount = totalCount,
                ResultPage = this.PrepareResults(dbpackages, prerelease).Skip(skip).Take(take)
            };

        public IEnumerable<SearchResultModel> PrepareResults(IEnumerable<Package> dbpackages, bool prerelease)
        {
            foreach (var dbpackage in dbpackages)
            {
                yield return new SearchResultModel
                {
                    Id = dbpackage.Id,
                    Version = dbpackage.Versions
                        .Where(x => !x.IsPrerelase || prerelease)
                        .Max(x => x.NuGetVersion)
                        .ToNormalizedString(),
                    Description = dbpackage.Description,
                    Versions = this.PrepareVersions(dbpackage.Versions, prerelease),
                    Authors = dbpackage.AuthorNames,
                    IconUrl = dbpackage.IconUrl,
                    LicenseUrl = dbpackage.LicenseUrl,
                    Owners = new[] { dbpackage.OwnerId },
                    ProjectUrl = dbpackage.ProjectUrl,
                    RegistrationUrl = this.Url.AbsoluteUrl(Routing.RegistrationsIndexRouteName, this.HttpContext, new
                    {
                        mode = RegistrationsContentMode.Plain,
                        id = dbpackage.IdLowercase
                    }),
                    Summary = dbpackage.Summary,
                    Tags = dbpackage.TagNames,
                    Title = dbpackage.Title,
                    DownloadCount = dbpackage.DownloadCount,
                    IsVerified = false
                };
            }
        }

        public IEnumerable<SearchVersionModel> PrepareVersions(IEnumerable<PackageVersion> dbversions, bool prerelease)
            => dbversions.Where(x => !x.IsPrerelase || prerelease).Select(x => new SearchVersionModel
            {
                RegistrationLeafUrl = this.Url.AbsoluteUrl(Routing.RegistrationsLeafRouteName, this.HttpContext, new
                {
                    mode = RegistrationsContentMode.Plain,
                    id = x.Package.IdLowercase,
                    version = x.VersionLowercase
                }),
                Version = x.Version,
                DownloadCount = x.DownloadCount
            });
    }
}
