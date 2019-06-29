namespace SlimGet.Data.Configuration
{
    public sealed class StorageConfiguration
    {
        public DatabaseConfiguration PostgreSQL { get; set; }
        public RedisConfiguration Redis { get; set; }
        public FileSystemConfiguration FileSystem { get; set; }
        public PackageStorageConfiguration Packages { get; set; }
    }
}
