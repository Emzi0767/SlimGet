using System;
using System.Buffers.Binary;

namespace SlimGet.Data
{
    public sealed class PdbParser : IDisposable
    {
        private MsfParser Parser { get; }

        private readonly bool _leaveOpen;
        private bool _isDisposed = false;

        public PdbParser(MsfParser msf, bool leaveOpen = true)
        {
            this._leaveOpen = leaveOpen;
            this.Parser = msf;
        }

        public bool TryGetMetadata(out PdbMetadata pdbMeta)
        {
            pdbMeta = default;
            Span<byte> buffer = stackalloc byte[(int)this.Parser.PageSize];
            if (!this.Parser.TryReadStream(1, buffer, out var written) || written < 28)
                return false;

            var age = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(8));
            var id = new Guid(buffer.Slice(12, 16));
            pdbMeta = new PdbMetadata(id, age);
            return true;
        }

        public void Dispose()
        {
            if (this._isDisposed)
                return;

            this._isDisposed = true;
            if (!this._leaveOpen)
                this.Parser.Dispose();
        }
    }
}
