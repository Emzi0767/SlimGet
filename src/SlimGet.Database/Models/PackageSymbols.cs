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
