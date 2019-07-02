using System;
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
        public string SymbolsFilename { get; set; }
        public Guid? SymbolsIdentifier { get; set; }
        public string SymbolsName { get; set; }

        public PackageVersion Package { get; set; }
        public PackageFramework PackageFramework { get; set; }

        public NuGetFramework NuGetFramework => NuGetFramework.Parse(this.Framework);
    }
}
