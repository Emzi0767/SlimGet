using System;

namespace SlimGet.Data.Database
{
    public sealed class PackageSymbols
    {
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }
        public string Framework { get; set; }
        public string BinaryName { get; set; }
        public Guid Identifier { get; set; }
        public int Age { get; set; }
        public SymbolKind Kind { get; set; }
        public string Signature { get; set; }
        public string Name { get; set; }
        public string Filename { get; set; }

        public PackageBinary Binary { get; set; }
    }
}
