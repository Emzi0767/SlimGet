namespace SlimGet.Data.Database
{
    public sealed class PackageDependency
    {
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }
        public string Id { get; set; }
        public string TargetFramework { get; set; }
        public string MinVersion { get; set; }
        public bool? IsMinVersionInclusive { get; set; }
        public string MaxVersion { get; set; }
        public bool? IsMaxVersionInclusive { get; set; }
        
        public PackageVersion Package { get; set; }
    }
}
