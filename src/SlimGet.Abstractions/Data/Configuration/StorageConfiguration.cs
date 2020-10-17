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

using System.ComponentModel.DataAnnotations;

namespace SlimGet.Data.Configuration
{
    /// <summary>
    /// Represents configuration for storage components of SlimGet.
    /// </summary>
    public sealed class StorageConfiguration
    {
        /// <summary>
        /// Gets or sets the database configuration.
        /// </summary>
        [Required]
        public DatabaseConfiguration Database { get; set; }

        /// <summary>
        /// Gets or sets the cache configuration.
        /// </summary>
        [Required]
        public CacheConfiguration Cache { get; set; }

        /// <summary>
        /// Gets or sets the blob storage configuration.
        /// </summary>
        [Required]
        public BlobStorageConfiguration Blobs { get; set; }

        /// <summary>
        /// Gets or sets the package storage rules configuration.
        /// </summary>
        [Required]
        public PackageStorageConfiguration Packages { get; set; }
    }
}
