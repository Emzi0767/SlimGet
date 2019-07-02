using System.Collections.Generic;
using NuGet.Frameworks;

namespace SlimGet.Data.Database
{
    public sealed class PackageFramework
    {
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }
        public string Framework { get; set; }

        public PackageVersion Package { get; set; }
        public List<PackageBinary> Binaries { get; set; }

        public NuGetFramework NuGetFramework => NuGetFramework.Parse(this.Framework);
    }
}
