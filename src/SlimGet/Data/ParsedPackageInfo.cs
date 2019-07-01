using System.Collections.Generic;
using NuGet.Frameworks;
using NuGet.Versioning;
using SlimGet.Data.Database;

namespace SlimGet.Data
{
    public class ParsedPackageInfo
    {
        // Package data
        public string Id { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string MinimumClientVersion { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string Summary { get; set; }
        public string Title { get; set; }
        public string IconUrl { get; set; }
        public string LicenseUrl { get; set; }
        public string ProjectUrl { get; set; }
        public string RepositoryUrl { get; set; }
        public string RepositoryType { get; set; }
        public SemVerLevel SemVerLevel { get; set; }
        public IEnumerable<string> Authors { get; set; }
        public IEnumerable<string> Tags { get; set; }

        // Version data
        public NuGetVersion Version { get; set; }
        public bool IsPrerelase { get; set; }
        public IEnumerable<ParsedDependencyInfo> Dependencies { get; set; }
        public IEnumerable<NuGetFramework> Frameworks { get; set; }

        public PackageInfo Info => new PackageInfo(this.Id, this.Version);
    }

    public sealed class ParsedDependencyInfo
    {
        public NuGetFramework Framework { get; set; }
        public string PackageId { get; set; }
        public NuGetVersion MinVersion { get; set; }
        public NuGetVersion MaxVersion { get; set; }
        public bool IsMinInclusive { get; set; }
        public bool IsMaxInclusive { get; set; }
    }
}
