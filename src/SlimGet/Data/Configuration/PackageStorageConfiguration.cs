namespace SlimGet.Data.Configuration
{
    public class PackageStorageConfiguration
    {
        public bool EnablePruning { get; set; }
        public int LatestVersionRetainCount { get; set; }
        public long MaxPackageSizeBytes { get; set; }
        public bool DeleteEndpointUnlists { get; set; }
        public bool ReadOnlyFeed { get; set; }
        public bool SymbolsEnabled { get; set; }
    }
}
