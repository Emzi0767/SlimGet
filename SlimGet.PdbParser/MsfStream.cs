using System.Collections.Immutable;

namespace SlimGet.Data
{
    public struct MsfStream
    {
        public uint ByteLength { get; }
        public int PageCount => this.Pages.Length;
        public ImmutableArray<uint> Pages { get; }

        public MsfStream(uint byteLength, ImmutableArray<uint> pages)
        {
            this.ByteLength = byteLength;
            this.Pages = pages;
        }
    }
}
