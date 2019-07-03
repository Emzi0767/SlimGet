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
using NuGet.Versioning;

namespace SlimGet.Data.Database
{
    public sealed class PackageVersion
    {
        public string PackageId { get; set; }
        public string Version { get; set; }
        public string VersionLowercase { get; set; }
        public long DownloadCount { get; set; }
        public bool IsPrerelase { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool IsListed { get; set; }
        public string PackageFilename { get; set; }
        public string ManifestFilename { get; set; }

        public Package Package { get; set; }
        public List<PackageDependency> Dependencies { get; set; }
        public List<PackageFramework> Frameworks { get; set; }
        public List<PackageBinary> Binaries { get; set; }

        public NuGetVersion NuGetVersion => NuGetVersion.TryParse(this.Version, out var ngv) ? ngv : null;
        public NuGetVersion NuGetVersionLowercase => NuGetVersion.TryParse(this.VersionLowercase, out var ngv) ? ngv : null;
    }
}
