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
    /// Represents the root of SlimGet configuration file.
    /// </summary>
    public sealed class SlimGetConfiguration
    {
        /// <summary>
        /// Gets or sets the storage configuration.
        /// </summary>
        [Required]
        public StorageConfiguration Storage { get; set; }

        /// <summary>
        /// Gets or sets the HTTP configuration.
        /// </summary>
        [Required]
        public HttpConfiguration Http { get; set; }

        /// <summary>
        /// Gets or sets the security configuration.
        /// </summary>
        [Required]
        public SecurityConfiguration Security { get; set; }
    }
}
