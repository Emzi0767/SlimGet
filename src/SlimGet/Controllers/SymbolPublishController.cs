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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimGet.Data;
using SlimGet.Data.Configuration;
using SlimGet.Filters;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [Route("/api/v2/symbolpackage"), ApiController, Authorize, ServiceFilter(typeof(RequireWritableFeed)), ServiceFilter(typeof(RequireSymbolsEnabled))]
    public class SymbolPublishController : NuGetControllerBase
    {
        private PackageProcessingService Packages { get; }

        public SymbolPublishController(SlimGetContext db, RedisService redis, IFileSystemService fs, IOptions<StorageConfiguration> storcfg, ILoggerFactory logger, PackageProcessingService pkgParser)
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
                    var pkgparse = await this.Packages.ParsePackageAsync(pkgtmp, spectmp, true, cancellationToken).ConfigureAwait(false);
                    if (pkgparse == null)
                        return this.BadRequest(new { message = "Package was malformed." });

                    pkgtmp.Position = 0;
                    spectmp.Position = 0;

                    var symbolFileMapping = pkgparse.Binaries
                        .OfType<ParsedIndexedBinarySymbols>()
                        .GroupBy(x => new SymbolIdentifier(x.Identifier, x.Age, x.Kind))
                        .ToDictionary(x => x.Key, x => this.FileSystem.GetSymbolsFileName(pkgparse.Info, x.Key.Identifier, x.Key.Age));

                    var result = await this.Packages.RegisterSymbolsAsync(pkgparse, this.Database, this.HttpContext.User.Identity.Name, symbolFileMapping, cancellationToken).ConfigureAwait(false);
                    if (result.Result == RegisterPackageResult.OwnerMismatch)
                        return this.StatusCode(403, new { message = "You are not the owner of this package." });

                    if (result.Result == RegisterPackageResult.IdMismatch)
                        return this.BadRequest(new { message = $"Package ID mismatch (check that package ID casing is identical)." });

                    if (result.Result == RegisterPackageResult.DoesNotExist)
                        return this.NotFound(new { message = "Specified package ID or version does not exist." });

                    if (result.Result == RegisterPackageResult.DuplicateSymbols)
                        return this.Conflict(new { message = "Debug symbols for specified package already exist." });

                    await this.Packages.ExtractSymbolsAsync(pkgtmp, pkgparse.Info, result.SymbolMappings, this.FileSystem, cancellationToken).ConfigureAwait(false);

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
                this.Logger.LogError(ex, "Error while registering symbols");
                return this.BadRequest();
            }
        }
    }
}
