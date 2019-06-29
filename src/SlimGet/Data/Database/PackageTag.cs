namespace SlimGet.Data.Database
{
    public sealed class PackageTag
    {
        public string PackageId { get; set; }
        public string Tag { get; set; }

        public Package Package { get; set; }
    }
}
