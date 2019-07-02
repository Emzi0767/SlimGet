using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
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
        /// <param name="symbolPackage">Whether the package is a symbol package.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>Parsed information.</returns>
        public async Task<ParsedPackageInfo> ParsePackageAsync(Stream pkgStream, Stream specStream, bool symbolPackage, CancellationToken cancellationToken)
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

                    var bins = await this.IndexBinaryContentsAsync(pkgReader, frameworks, symbolPackage, cancellationToken).ConfigureAwait(false);
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
        /// <returns>Registration result.</returns>
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

            foreach (var bin in packageInfo.Binaries.OfType<ParsedIndexedBinaryExecutable>())
            {
                await database.PackageBinaries.AddAsync(new PackageBinary
                {
                    PackageId = pkginfo.Id,
                    PackageVersion = pkginfo.NormalizedVersion,
                    Framework = bin.Framework.GetFrameworkString(),
                    Name = bin.Entry,
                    Length = bin.Length,
                    Hash = bin.Sha256
                }, cancellationToken).ConfigureAwait(false);

                foreach (var symbolId in bin.SymbolIdentifiers)
                    await database.PackageSymbols.AddAsync(new PackageSymbols
                    {
                        PackageId = pkginfo.Id,
                        PackageVersion = pkginfo.NormalizedVersion,
                        Framework = bin.Framework.GetFrameworkString(),
                        BinaryName = bin.Entry,
                        Identifier = symbolId
                    }, cancellationToken).ConfigureAwait(false);
            }

            await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Registers debug symbols with the package database.
        /// </summary>
        /// <param name="packageInfo">Metadata of the package to register.</param>
        /// <param name="database">Database to register to.</param>
        /// <param name="symbolFileNames">Mapping of symbol id to symbol virtual identifiers.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns>Mapping of registered symbol identifiers to their entry names in the package.</returns>
        public async Task<Dictionary<Guid, string>> RegisterSymbolsAsync(ParsedPackageInfo packageInfo, SlimGetContext database, IDictionary<Guid, string> symbolFileNames, CancellationToken cancellationToken)
        {
            var pkginfo = packageInfo.Info;
            var dbsymbols = database.PackageSymbols
                .Where(x => x.PackageId == pkginfo.Id && x.PackageVersion == pkginfo.NormalizedVersion)
                .GroupBy(x => x.Identifier)
                .ToDictionary(x => x.Key, x => x);

            var regids = new Dictionary<Guid, string>();
            foreach (var bin in packageInfo.Binaries.OfType<ParsedIndexedBinarySymbols>())
            {
                if (!dbsymbols.TryGetValue(bin.Identifier, out var dbsymbolg) || !symbolFileNames.TryGetValue(bin.Identifier, out var fnsymbol))
                    continue;

                var fx = bin.Framework.GetFrameworkString();
                var dbsymbol = dbsymbolg.FirstOrDefault(x => x.Framework == fx);
                if (dbsymbol == null)
                    continue;

                regids[bin.Identifier] = bin.Entry;
                dbsymbol.Name = bin.Name;
                dbsymbol.Filename = fnsymbol;
                database.PackageSymbols.Update(dbsymbol);
            }

            await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return regids;
        }

        /// <summary>
        /// Extracts symbol files from the package.
        /// </summary>
        /// <param name="pkgStream">Package to extract symbols from.</param>
        /// <param name="pkgInfo">Information about package to which the symbols belong.</param>
        /// <param name="entryIdMap">Mapping of entry to id.</param>
        /// <param name="fs">Filesystem to write the symbols to.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
        /// <returns></returns>
        public async Task ExtractSymbolsAsync(Stream pkgStream, PackageInfo pkgInfo, IDictionary<Guid, string> entryIdMap, IFileSystemService fs, CancellationToken cancellationToken)
        {
            using (var pkg = new PackageArchiveReader(pkgStream, true))
            {
                foreach (var (id, entry) in entryIdMap)
                {
                    var ze = pkg.GetEntry(entry);
                    using (var pdbIn = ze.Open())
                    using (var pdbOut = fs.OpenSymbolsWrite(pkgInfo, id))
                        await pdbIn.CopyToAsync(pdbOut, cancellationToken).ConfigureAwait(false);
                }
            }
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

        private async Task<IEnumerable<BaseParsedIndexedBinary>> IndexBinaryContentsAsync(PackageArchiveReader package, IEnumerable<NuGetFramework> supportedFrameworks, bool symbolPackage, CancellationToken cancellationToken)
        {
            var files = (await package.GetFilesAsync(cancellationToken).ConfigureAwait(false))
                .Select(x => new { name = x, ext = Path.GetExtension(x) });

            var hasSymbols = files.Any(x => string.Equals(x.ext, ".pdb", StringComparison.InvariantCultureIgnoreCase));

            if (!symbolPackage && hasSymbols)
                return null;
            else if (symbolPackage && !hasSymbols)
                return null;

            var skipFx = supportedFrameworks.Contains(NuGetFramework.AnyFramework);
            var bins = files.Where(x => string.Equals(x.ext, ".dll", StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(x.ext, ".exe", StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(x.ext, ".pdb", StringComparison.InvariantCultureIgnoreCase));

            var binaries = new List<BaseParsedIndexedBinary>();
            foreach (var bin in bins)
            {
                var entry = bin.name;
                var name = Path.GetFileName(entry);
                var ext = bin.ext;
                var location = Path.GetDirectoryName(entry);
                var parent = Path.GetFileName(location);
                var hash = default(string);
                var fx = string.Equals(parent, "lib", StringComparison.InvariantCultureIgnoreCase) || skipFx ? NuGetFramework.AnyFramework : NuGetFramework.ParseFolder(parent);
                var isSymbols = string.Equals(ext, ".pdb", StringComparison.InvariantCultureIgnoreCase);

                if (!supportedFrameworks.Contains(fx))
                    continue;

                var ze = package.GetEntry(entry);
                using (var ms = new MemoryStream())
                {
                    using (var zstream = ze.Open())
                        await zstream.CopyToAsync(ms).ConfigureAwait(false);
                    ms.Position = 0;

                    if (!isSymbols)
                    {
                        using (var sha256 = SHA256.Create())
                        {
                            var hashBin = sha256.ComputeHash(ms);
                            hash = string.Create(256 / 8 * 2, hashBin, StringifyHash);
                        }
                        ms.Position = 0;

                        using (var pereader = new PEReader(ms))
                        {
                            var symbolIds = default(IEnumerable<Guid>);

                            var debugdir = pereader.ReadDebugDirectory();
                            if (debugdir != null && debugdir.Length > 0)
                                symbolIds = debugdir.Where(x => x.Type == DebugDirectoryEntryType.CodeView)
                                    .Select(x => pereader.ReadCodeViewDebugDirectoryData(x).Guid);

                            binaries.Add(new ParsedIndexedBinaryExecutable
                            {
                                Entry = entry,
                                Name = name,
                                Extension = ext,
                                Location = location,
                                Parent = parent,
                                Sha256 = hash,
                                Length = ze.Length,
                                Framework = fx,
                                SymbolIdentifiers = symbolIds.ToList() // Otherwise it tries to enumerate over disposed objects
                            });
                        }
                    }
                    else
                    {
                        using (var metadata = MetadataReaderProvider.FromPortablePdbStream(ms, MetadataStreamOptions.LeaveOpen))
                        {
                            var reader = metadata.GetMetadataReader();
                            var dbgHeader = reader.DebugMetadataHeader;
                            if (dbgHeader != null)
                                binaries.Add(new ParsedIndexedBinarySymbols
                                {
                                    Entry = entry,
                                    Name = name,
                                    Extension = ext,
                                    Location = location,
                                    Parent = parent,
                                    Framework = fx,
                                    Identifier = new BlobContentId(dbgHeader.Id).Guid
                                });
                        }
                    }
                }
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
