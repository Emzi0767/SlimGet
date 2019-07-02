using SlimGet.Data;

namespace SlimGet.Services
{
    public sealed class PackageKeyProvider
    {
        public string GetPackageKey(PackageInfo packageInfo, KeyType keyType)
            => $"slimget::packages::{packageInfo.Id}::properties::{keyType}";

        public string GetVersionKey(PackageInfo packageInfo, KeyType keyType)
            => $"slimget::packages::{packageInfo.Id}::versions::{packageInfo.NormalizedVersion}::properties::{keyType}";
    }

    public enum KeyType
    {
        DownloadCount
    }
}
