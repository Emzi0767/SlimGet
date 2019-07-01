using System;
using System.Collections.Generic;
using NuGet.Versioning;

namespace SlimGet.Data.Database
{
    public sealed class PackageVersion
    {
        public string PackageId { get; set; }
        public string Version { get; set; }
        public string VersionLowercase { get; set; }
        public long DownloadCount { get; set; }
        public bool IsPrerelase { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsListed { get; set; }
        public string PackageFileName { get; set; }
        public string ManifestFileName { get; set; }
        public string SymbolsFileName { get; set; }
        public Guid? SymbolsIdentifier { get; set; }

        public Package Package { get; set; }
        public List<PackageDependency> Dependencies { get; set; }
        public List<PackageFramework> Frameworks { get; set; }

        public NuGetVersion NuGetVersion => NuGetVersion.TryParse(this.Version, out var ngv) ? ngv : null;
    }
}
