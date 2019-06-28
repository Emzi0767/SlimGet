namespace SlimGet.Data.Database
{
    public sealed class PackageAuthor
    {
        public string PackageId { get; set; }
        public string Name { get; set; }

        public Package Package { get; set; }
    }
}
