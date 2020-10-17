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
    /// Represents configuration for cache backend.
    /// </summary>
    public sealed class CacheConfiguration
    {
        /// <summary>
        /// Gets or sets the type of the implementation.
        /// </summary>
        [Required]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the hostname to connect to.
        /// </summary>
        [Required]
        public string Hostname { get; set; }

        /// <summary>
        /// Gets or sets the port to connect to.
        /// </summary>
        [Required]
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the database index to use.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the password to authenticate with.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets whether to use SSL for the connection.
        /// </summary>
        public bool UseSsl { get; set; }
    }
}
