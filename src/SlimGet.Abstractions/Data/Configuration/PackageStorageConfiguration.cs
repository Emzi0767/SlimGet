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

namespace SlimGet.Data.Configuration
{
    /// <summary>
    /// Represents options for the virtual package storage, which define various rules such as retention, etc.
    /// </summary>
    public class PackageStorageConfiguration
    {
        /// <summary>
        /// Gets or sets whether to enable pruning old versions.
        /// </summary>
        public bool EnablePruning { get; set; }

        /// <summary>
        /// Gets or sets the number of latest package versions to retain. This setting has no effect is
        /// <see cref="EnablePruning"/> is set to <see langword="false"/>.
        /// </summary>
        public int LatestVersionRetainCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum package size, in bytes. This applies to a single nupkg file.
        /// </summary>
        public long MaxPackageSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets whether the delete endpoint will unlist packages, or delete them.
        /// </summary>
        public bool DeleteEndpointUnlists { get; set; }

        /// <summary>
        /// Gets or sets whether the feed is readonly.
        /// </summary>
        public bool ReadOnlyFeed { get; set; }

        /// <summary>
        /// Gets or sets whether the symbol server is enabled.
        /// </summary>
        public bool SymbolsEnabled { get; set; }
    }
}
