using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Frameworks;
using NuGet.Packaging;
using SlimGet.Data;
using SlimGet.Data.Database;

namespace SlimGet.Services
{
    public sealed class PackageProcessingService
    {
        private ILogger<PackageProcessingService> Logger { get; }

        public PackageProcessingService(ILoggerFactory logger)
        {
            this.Logger = logger.CreateLogger<PackageProcessingService>();
        }

        /// <summary>
        /// Parses package information.
        /// </summary>
        /// <param name="pkgStream">Stream containing the package to parse.</param>
        /// <param name="specStream">Stream to which manifest will be written.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>Parsed information.</returns>
        public async Task<ParsedPackageInfo> ParsePackageAsync(Stream pkgStream, Stream specStream, CancellationToken cancellationToken)
        {
            try
            {
                using (var pkgReader = new PackageArchiveReader(pkgStream, true))
                {
                    using (var pkgSpec = await pkgReader.GetNuspecAsync(cancellationToken).ConfigureAwait(false))
                        await pkgSpec.CopyToAsync(specStream).ConfigureAwait(false);

                    var nuspec = await pkgReader.GetNuspecReaderAsync(cancellationToken).ConfigureAwait(false);

                    var frameworks = await pkgReader.GetSupportedFrameworksAsync(cancellationToken).ConfigureAwait(false);
                    if (!frameworks.Any())
                        frameworks = new[] { NuGetFramework.AnyFramework };

                    var bins = await this.IndexBinaryContentsAsync(pkgReader, frameworks, false, cancellationToken).ConfigureAwait(false);
                    if (bins == null)
                        return null;

                    var repometa = nuspec.GetRepositoryMetadata();
                    this.ProcessDependencies(nuspec, out var deps, out var semver);
                    return new ParsedPackageInfo
                    {
                        Id = nuspec.GetId(),
                        Description = nuspec.GetDescription(),
                        Language = nuspec.GetLanguage(),
                        MinimumClientVersion = nuspec.GetMinClientVersion()?.ToNormalizedString(),
                        RequireLicenseAcceptance = nuspec.GetRequireLicenseAcceptance(),
                        Summary = nuspec.GetSummary(),
                        Title = nuspec.GetTitle(),
                        IconUrl = nuspec.GetIconUrl(),
                        LicenseUrl = nuspec.GetLicenseUrl(),
                        RepositoryUrl = repometa?.Url,
                        RepositoryType = repometa?.Type,
                        SemVerLevel = nuspec.GetVersion().IsSemVer2 ? SemVerLevel.SemVer_2_0_0 : semver,
                        Authors = nuspec.GetAuthors().Split(new[] { ',', ';', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries),
                        Tags = nuspec.GetTags().Split(new[] { ',', ';', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries),
                        Version = nuspec.GetVersion(),
                        IsPrerelase = nuspec.GetVersion().IsPrerelease,
                        Dependencies = deps,
                        Frameworks = frameworks,
                        Binaries = bins
                    };
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Could not parse uploaded package");
                return null;
            }
        }

        /// <summary>
        /// Registers a package with the package database.
        /// </summary>
        /// <param name="packageInfo">Metadata of the package to register.</param>
        /// <param name="database">Database to register to.</param>
        /// <param name="userId">ID of the user uploading the package.</param>
        /// <param name="packageFileName">Virtual file ID of the package file.</param>
        /// <param name="manifestFileName">Virtual file ID of the manifest file.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>Registration result</returns>
        public async Task<RegisterPackageResult> RegisterPackageAsync(ParsedPackageInfo packageInfo, SlimGetContext database, string userId, string packageFileName, string manifestFileName,
            CancellationToken cancellationToken)
        {
            var pkginfo = packageInfo.Info;
            var pkg = database.Packages.FirstOrDefault(x => x.IdLowercase == pkginfo.IdLowercase);
            var pkgv = default(PackageVersion);

            var result = RegisterPackageResult.VersionCreated;
            if (pkg != null)
            {
                if (pkg.OwnerId != userId)
                    return RegisterPackageResult.OwnerMismatch;

                if (pkg.Id != pkginfo.Id)
                    return RegisterPackageResult.IdMismatch;

                pkgv = database.PackageVersions.FirstOrDefault(x => x.PackageId == pkginfo.Id && x.Version == pkginfo.NormalizedVersion);
                if (pkgv != null)
                    return RegisterPackageResult.AlreadyExists;

                // package exists, ownership matches, id matches, version does not exist, update and create

                pkg.Description = packageInfo.Description;
                pkg.Language = packageInfo.Language;
                pkg.MinimumClientVersion = packageInfo.MinimumClientVersion;
                pkg.RequireLicenseAcceptance = packageInfo.RequireLicenseAcceptance;
                pkg.Summary = packageInfo.Summary;
                pkg.Title = packageInfo.Title;
                pkg.IconUrl = packageInfo.IconUrl;
                pkg.LicenseUrl = packageInfo.LicenseUrl;
                pkg.ProjectUrl = packageInfo.ProjectUrl;
                pkg.RepositoryUrl = packageInfo.RepositoryUrl;
                pkg.RepositoryType = packageInfo.RepositoryType;
                pkg.SemVerLevel = packageInfo.SemVerLevel;
                database.Packages.Update(pkg);

                var oldAuthors = database.PackageAuthors.Where(x => x.PackageId == pkginfo.Id);
                database.PackageAuthors.RemoveRange(oldAuthors);

                var oldTags = database.PackageTags.Where(x => x.PackageId == pkginfo.Id);
                database.PackageTags.RemoveRange(oldTags);
            }
            else
            {
                // package does not exist, create it
                result = RegisterPackageResult.PackageCreated;

                pkg = new Package
                {
                    Id = pkginfo.Id,
                    IdLowercase = pkginfo.IdLowercase,
                    Description = packageInfo.Description,
                    DownloadCount = 0,
                    Language = packageInfo.Language,
                    IsListed = true,
                    MinimumClientVersion = packageInfo.MinimumClientVersion,
                    PublishedAt = DateTime.UtcNow,
                    RequireLicenseAcceptance = packageInfo.RequireLicenseAcceptance,
                    Summary = packageInfo.Summary,
                    Title = packageInfo.Title,
                    IconUrl = packageInfo.IconUrl,
                    LicenseUrl = packageInfo.LicenseUrl,
                    ProjectUrl = packageInfo.ProjectUrl,
                    RepositoryUrl = packageInfo.RepositoryUrl,
                    RepositoryType = packageInfo.RepositoryType,
                    SemVerLevel = packageInfo.SemVerLevel,
                    OwnerId = userId
                };
                await database.Packages.AddAsync(pkg, cancellationToken).ConfigureAwait(false);
            }

            foreach (var author in packageInfo.Authors)
                await database.PackageAuthors.AddAsync(new PackageAuthor
                {
                    PackageId = pkginfo.Id,
                    Name = author
                }, cancellationToken).ConfigureAwait(false);

            foreach (var tag in packageInfo.Tags)
                await database.PackageTags.AddAsync(new PackageTag
                {
                    PackageId = pkginfo.Id,
                    Tag = tag
                }, cancellationToken).ConfigureAwait(false);

            pkgv = new PackageVersion
            {
                PackageId = pkginfo.Id,
                Version = pkginfo.NormalizedVersion,
                VersionLowercase = pkginfo.NormalizedVersion.ToLowerInvariant(),
                DownloadCount = 0,
                IsPrerelase = packageInfo.IsPrerelase,
                PublishedAt = DateTime.UtcNow,
                IsListed = true,
                PackageFilename = packageFileName,
                ManifestFilename = manifestFileName
            };
            await database.PackageVersions.AddAsync(pkgv, cancellationToken).ConfigureAwait(false);

            foreach (var fx in packageInfo.Frameworks)
                await database.PackageFrameworks.AddAsync(new PackageFramework
                {
                    PackageId = pkginfo.Id,
                    PackageVersion = pkginfo.NormalizedVersion,
                    Framework = fx.GetFrameworkString()
                }, cancellationToken).ConfigureAwait(false);

            foreach (var dep in packageInfo.Dependencies)
                await database.PackageDependencies.AddAsync(new PackageDependency
                {
                    PackageId = pkginfo.Id,
                    PackageVersion = pkginfo.NormalizedVersion,
                    TargetFramework = dep.Framework.GetFrameworkString(),
                    Id = dep.PackageId,
                    MaxVersion = dep.MaxVersion?.ToNormalizedString(),
                    MinVersion = dep.MinVersion?.ToNormalizedString(),
                    IsMaxVersionInclusive = dep.IsMaxInclusive,
                    IsMinVersionInclusive = dep.IsMinInclusive
                }, cancellationToken).ConfigureAwait(false);

            foreach (var bin in packageInfo.Binaries)
                await database.PackageBinaries.AddAsync(new PackageBinary
                {
                    PackageId = pkginfo.Id,
                    PackageVersion = pkginfo.NormalizedVersion,
                    Framework = bin.Framework.GetFrameworkString(),
                    Name = bin.Entry,
                    Length = bin.Length,
                    Hash = bin.Sha256,
                    SymbolsFilename = null,
                    SymbolsIdentifier = null,
                    SymbolsName = null
                }, cancellationToken).ConfigureAwait(false);

            await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Prunes old packages that exceed the maximum version count.
        /// </summary>
        /// <param name="id">Id of the package to prune.</param>
        /// <param name="maxCount">Maximum number of package versions to retain.</param>
        /// <param name="database">Database to check for registrations.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>Pruned package information objects.</returns>
        public async Task<IEnumerable<PackageInfo>> PrunePackageAsync(string id, int maxCount, SlimGetContext database, CancellationToken cancellationToken)
        {
            var versions = database.PackageVersions.Where(x => x.PackageId == id).OrderByDescending(x => x.NuGetVersion);
            if (versions.Count() <= maxCount)
                return Enumerable.Empty<PackageInfo>();

            var prunable = versions.Skip(maxCount);
            var pruned = prunable.Select(x => new PackageInfo(x.PackageId, x.NuGetVersion));
            database.RemoveRange(prunable);
            await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return pruned;
        }

        private void ProcessDependencies(NuspecReader nuspec, out IEnumerable<ParsedDependencyInfo> dependencies, out SemVerLevel semVerLevel)
        {
            semVerLevel = SemVerLevel.SemVer_2_0_0;
            var deps = new List<ParsedDependencyInfo>();
            dependencies = deps;
            foreach (var depgroup in nuspec.GetDependencyGroups())
            {
                foreach (var dep in depgroup.Packages)
                {
                    deps.Add(new ParsedDependencyInfo
                    {
                        Framework = depgroup.TargetFramework,
                        PackageId = dep.Id,
                        MinVersion = dep.VersionRange.MinVersion,
                        MaxVersion = dep.VersionRange.MaxVersion,
                        IsMinInclusive = dep.VersionRange.IsMinInclusive,
                        IsMaxInclusive = dep.VersionRange.IsMaxInclusive
                    });
                }
            }
        }

        private async Task<IEnumerable<ParsedIndexedBinary>> IndexBinaryContentsAsync(PackageArchiveReader package, IEnumerable<NuGetFramework> supportedFrameworks, bool canContainSymbols, CancellationToken cancellationToken)
        {
            var files = await package.GetFilesAsync(cancellationToken).ConfigureAwait(false);
            if (!canContainSymbols && files.Any(x => string.Compare(Path.GetExtension(x), ".pdb", true, CultureInfo.InvariantCulture) == 0))
                return null;

            var skipFx = supportedFrameworks.Contains(NuGetFramework.AnyFramework);
            var bins = files.Select(x => new { name = x, ext = Path.GetExtension(x) })
                .Where(x => string.Compare(x.ext, ".dll", true, CultureInfo.InvariantCulture) == 0 || string.Compare(x.ext, ".exe", true, CultureInfo.InvariantCulture) == 0);

            var binaries = new List<ParsedIndexedBinary>();
            foreach (var bin in bins)
            {
                var entry = bin.name;
                var name = Path.GetFileName(entry);
                var ext = bin.ext;
                var location = Path.GetDirectoryName(entry);
                var parent = Path.GetFileName(location);
                var hash = default(string);
                var fx = string.Compare(parent, "lib", true, CultureInfo.InvariantCulture) == 0 || skipFx ? NuGetFramework.AnyFramework : NuGetFramework.ParseFolder(parent);

                if (!supportedFrameworks.Contains(fx))
                    continue;

                var ze = package.GetEntry(entry);
                using (var zstream = ze.Open())
                using (var sha256 = SHA256.Create())
                {
                    var hashBin = sha256.ComputeHash(zstream);
                    hash = string.Create(256 / 8 * 2, hashBin, StringifyHash);
                }
                // Possibly think of adding pulling the symbols from the files?
                // Or just leave it to the symbols controller? If hashes match,
                // that doesn't matter, right?

                binaries.Add(new ParsedIndexedBinary
                {
                    Entry = entry,
                    Name = name,
                    Extension = ext,
                    Location = location,
                    Parent = parent,
                    Sha256 = hash,
                    Length = ze.Length,
                    Framework = fx,
                });
            }

            return binaries;

            void StringifyHash(Span<char> buffer, byte[] state)
            {
                for (var i = state.Length - 1; i >= 0; i--)
                    state[i].TryFormat(buffer.Slice(i * 2), out _, "x2", CultureInfo.InvariantCulture);
            }
        }
    }

    public enum RegisterPackageResult
    {
        OwnerMismatch,
        IdMismatch,
        AlreadyExists,
        PackageCreated,
        VersionCreated
    }
}
