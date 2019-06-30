using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlimGet.Data.Database
{
    public sealed class PackageDependency
    {
        public string Id { get; set; }
        public string VersionRange { get; set; }
        public string TargetFramework { get; set; }
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }

        public PackageVersion Package { get; set; }
    }
}
