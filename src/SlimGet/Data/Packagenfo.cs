using NuGet.Versioning;

namespace SlimGet.Data
{
    public struct PackageInfo
    {
        public string Id { get; }
        public NuGetVersion Version { get; }

        public PackageInfo(string id, NuGetVersion version)
        {
            this.Id = id;
            this.Version = version;
        }
    }
}
