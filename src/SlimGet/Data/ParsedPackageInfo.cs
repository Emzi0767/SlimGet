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
using System.Collections.Generic;
using NuGet.Frameworks;
using NuGet.Versioning;
using SlimGet.Data.Database;

namespace SlimGet.Data
{
    public class ParsedPackageInfo
    {
        // Package data
        public string Id { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string MinimumClientVersion { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string Summary { get; set; }
        public string Title { get; set; }
        public string IconUrl { get; set; }
        public string LicenseUrl { get; set; }
        public string ProjectUrl { get; set; }
        public string RepositoryUrl { get; set; }
        public string RepositoryType { get; set; }
        public SemVerLevel SemVerLevel { get; set; }
        public IEnumerable<string> Authors { get; set; }
        public IEnumerable<string> Tags { get; set; }

        // Version data
        public NuGetVersion Version { get; set; }
        public bool IsPrerelase { get; set; }
        public IEnumerable<ParsedDependencyInfo> Dependencies { get; set; }
        public IEnumerable<NuGetFramework> Frameworks { get; set; }

        // Indexed binaries
        public IEnumerable<BaseParsedIndexedBinary> Binaries { get; set; }

        public PackageInfo Info => new PackageInfo(this.Id, this.Version);
    }

    public sealed class ParsedDependencyInfo
    {
        public NuGetFramework Framework { get; set; }
        public string PackageId { get; set; }
        public NuGetVersion MinVersion { get; set; }
        public NuGetVersion MaxVersion { get; set; }
        public bool IsMinInclusive { get; set; }
        public bool IsMaxInclusive { get; set; }
    }

    public abstract class BaseParsedIndexedBinary
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Location { get; set; }
        public string Parent { get; set; }
        public string Entry { get; set; }
        public NuGetFramework Framework { get; set; }
    }

    public sealed class ParsedIndexedBinaryExecutable : BaseParsedIndexedBinary
    {
        public string Sha256 { get; set; }
        public long Length { get; set; }
        public IEnumerable<SymbolIdentifier> SymbolIdentifiers { get; set; }
    }

    public sealed class ParsedIndexedBinarySymbols : BaseParsedIndexedBinary
    {
        public Guid Identifier { get; set; }
        public int Age { get; set; }
        public SymbolKind Kind { get; set; }
    }
}
