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
using System.Collections.Immutable;
using System.IO;
using System.Runtime.InteropServices;

namespace SlimGet.Data
{
    // https://code.google.com/archive/p/pdbparser/wikis/MSF_Format.wiki
    // https://llvm.org/docs/PDB/MsfFile.html#msf-superblock
    // https://llvm.org/docs/PDB/index.html
    public sealed class MsfParser : IDisposable
    {
        private static byte[] Magic { get; } = new byte[]
        {
            0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66,
            0x74, 0x20, 0x43, 0x2F, 0x43, 0x2B, 0x2B, 0x20,
            0x4D, 0x53, 0x46, 0x20, 0x37, 0x2E, 0x30, 0x30,
            0x0D, 0x0A, 0x1A, 0x44, 0x53, 0x00, 0x00, 0x00
        }; /* "Microsoft C/C++ MSF 7.00\r\n\032DS\0\0" */

        private Stream InputStream { get; }

        /// <summary>
        /// Gets the byte number of individual page, in bytes. Total file size is always a multiple of this, usually (page size * page count).
        /// </summary>
        public uint PageSize { get; }

        /// <summary>
        /// Gets the page number of the free page map.
        /// </summary>
        public uint FreePageMapPage { get; }

        /// <summary>
        /// Gets the number of pages in the MSF container.
        /// </summary>
        public uint PageCount { get; }

        /// <summary>
        /// Gets the size of the root directory, bytes.
        /// </summary>
        public uint RootDirectorySize { get; }

        /// <summary>
        /// Gets page number of the page map.
        /// </summary>
        public uint PageMapAddress { get; }

        /// <summary>
        /// Gets the array of page numbers containing the root directory.
        /// </summary>
        private ImmutableArray<uint> RootDirectoryPointers { get; }

        /// <summary>
        /// Gets the mapping of streams to pages.
        /// </summary>
        private ImmutableArray<MsfStream> StreamMap { get; }

        private readonly bool _leaveOpen;
        private bool _isDisposed = false;

        public MsfParser(Stream inputStream, bool leaveOpen = true)
        {
            this.InputStream = inputStream;
            this._leaveOpen = leaveOpen;

            Span<byte> buff = stackalloc byte[4];
            if (!this.ValidateMsf() ||
                !this.TryReadDword(buff, out var pageSize) ||
                !this.TryReadDword(buff, out var freePageMapPage) ||
                !this.TryReadDword(buff, out var pageCount) ||
                !this.TryReadDword(buff, out var rootDirSize) ||
                !this.TryReadDword(buff, out _) ||
                !this.TryReadDword(buff, out var pageMapAddress) ||
                this.InputStream.Length != pageCount * pageSize)
                throw new InvalidDataException("Invalid MSF file supplied.");

            var pageMapArraySize = rootDirSize / pageSize + (rootDirSize % pageSize != 0 ? 1 : 0);
            this.InputStream.Position = pageMapAddress * pageSize;
            if (!this.TryReadDwordArray((int)pageMapArraySize, out var rootDirPtrs))
                throw new InvalidDataException("Invalid MSF file supplied.");

            this.PageSize = pageSize;
            this.FreePageMapPage = freePageMapPage;
            this.PageCount = pageCount;
            this.RootDirectorySize = rootDirSize;
            this.RootDirectoryPointers = rootDirPtrs;
            this.PageMapAddress = pageMapAddress;
            this.RootDirectoryPointers = rootDirPtrs;

            this.InputStream.Position = rootDirPtrs[0] * pageSize;
            if (!this.TryReadStreamMap(buff, out var streamMap))
                throw new InvalidDataException("Invalid MSF file supplied.");

            this.StreamMap = streamMap;
        }

        public bool TryReadStream(int index, Span<byte> destination, out int written)
        {
            written = 0;
            if (index >= this.StreamMap.Length)
                return false;

            var stream = this.StreamMap[index];
            if (destination.Length < stream.ByteLength)
                return false;

            written = (int)stream.ByteLength;
            var cdest = destination;
            foreach (var page in stream.Pages)
            {
                this.InputStream.Position = page * this.PageSize;
                if (this.InputStream.Read(cdest.Slice(0, (int)this.PageSize)) != this.PageSize)
                    return false;

                cdest = cdest.Slice((int)this.PageSize);
            }

            return true;
        }

        public void Dispose()
        {
            if (this._isDisposed)
                return;

            this._isDisposed = true;
            if (!this._leaveOpen)
                this.InputStream.Dispose();
        }

        private bool ValidateMsf()
        {
            Span<byte> magic = stackalloc byte[Magic.Length];
            if (this.InputStream.Read(magic) != Magic.Length)
                return false;

            return magic.SequenceEqual(Magic);
        }

        private bool TryReadDword(Span<byte> buff, out uint value)
        {
            value = default;
            if (this.InputStream.Read(buff) != 4)
                return false;

            value = BinaryPrimitives.ReadUInt32LittleEndian(buff);
            return true;
        }

        private bool TryReadDwordArray(int size, out ImmutableArray<uint> values)
        {
            values = default;

            // Read in 64k chunks, reinterpreting as UInts
            Span<byte> buff = stackalloc byte[size * 4];
            var remaining = size * 4;
            var results = ImmutableArray.CreateBuilder<uint>(size);
            var isLE = BitConverter.IsLittleEndian;

            while (remaining > 0)
            {
                var chunkSize = Math.Min(remaining, buff.Length);
                var xbuff = chunkSize != buff.Length ? buff.Slice(0, chunkSize) : buff;

                if (this.InputStream.Read(xbuff) != chunkSize)
                    return false;

                if (isLE)
                {
                    results.AddRange(MemoryMarshal.Cast<byte, uint>(xbuff).ToArray(), chunkSize / 4);
                }
                else
                {
                    for (var i = chunkSize / 4 - 1; i >= 0; i--)
                        results.Add(BinaryPrimitives.ReadUInt32LittleEndian(xbuff.Slice(i * 4)));
                }

                remaining -= chunkSize;
            }

            values = results.ToImmutable();
            return true;
        }

        private bool TryReadStreamMap(Span<byte> buff, out ImmutableArray<MsfStream> streamMap)
        {
            streamMap = default;
            if (!this.TryReadDword(buff, out var scount))
                return false;

            if (!this.TryReadDwordArray((int)scount, out var lengths))
                return false;

            var maps = ImmutableArray.CreateBuilder<MsfStream>((int)scount);
            foreach (var len in lengths)
            {
                var pcount = len / this.PageSize + (len % this.PageSize != 0 ? 1 : 0);
                if (!this.TryReadDwordArray((int)pcount, out var pages))
                    return false;

                maps.Add(new MsfStream(len, pages));
            }

            streamMap = maps.ToImmutable();
            return true;
        }
    }
}
