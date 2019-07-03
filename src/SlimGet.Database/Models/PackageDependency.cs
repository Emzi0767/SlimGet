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

namespace SlimGet.Data.Database
{
    public sealed class PackageDependency
    {
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }
        public string Id { get; set; }
        public string TargetFramework { get; set; }
        public string MinVersion { get; set; }
        public bool? IsMinVersionInclusive { get; set; }
        public string MaxVersion { get; set; }
        public bool? IsMaxVersionInclusive { get; set; }
        
        public PackageVersion Package { get; set; }
    }
}
