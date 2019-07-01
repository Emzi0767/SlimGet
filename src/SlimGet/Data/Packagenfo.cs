using NuGet.Versioning;

namespace SlimGet.Data
{
    public struct PackageInfo
    {
        public string Id { get; }
        public string IdLowercase => this.Id.ToLowerInvariant();
        public NuGetVersion Version { get; }
        public string NormalizedVersion => this.Version.ToNormalizedString();

        public PackageInfo(string id, NuGetVersion version)
        {
            this.Id = id;
            this.Version = version;
        }
    }
}
