using System.Collections.Generic;
using NuGet.Frameworks;

namespace SlimGet.Data.Database
{
    public sealed class PackageBinary
    {
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }
        public string Framework { get; set; }
        public string Name { get; set; }
        public long Length { get; set; }
        public string Hash { get; set; }

        public PackageVersion Package { get; set; }
        public PackageFramework PackageFramework { get; set; }
        public List<PackageSymbols> PackageSymbols { get; set; }

        public NuGetFramework NuGetFramework => NuGetFramework.Parse(this.Framework);
    }
}
