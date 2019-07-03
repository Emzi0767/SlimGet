// This file is a part of SlimGet project.
//
// Copyright 2019 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
