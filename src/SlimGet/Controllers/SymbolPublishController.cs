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
                        .GroupBy(x => x.Identifier)
                        .ToDictionary(x => x.Key, x => this.FileSystem.GetSymbolsFileName(pkgparse.Info, x.Key));

                    var regids = await this.Packages.RegisterSymbolsAsync(pkgparse, this.Database, symbolFileMapping, cancellationToken).ConfigureAwait(false);
                    await this.Packages.ExtractSymbolsAsync(pkgtmp, pkgparse.Info, regids, this.FileSystem, cancellationToken).ConfigureAwait(false);

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
