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
    /// Represents an HTTP listen endpoint.
    /// </summary>
    public class HttpEndpointConfiguration
    {
        /// <summary>
        /// Gets the address to bind to.
        /// </summary>
        [Required]
        public string Address { get; set; }

        /// <summary>
        /// Gets the port to bind to.
        /// </summary>
        [Required, Range(1, 65535)]
        public int Port { get; set; }

        /// <summary>
        /// Gets whether requests served on this endpoint should use encryption.
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Gets the path to certificate file to use for this endpoint.
        /// </summary>
        public string CertificateFile { get; set; }

        /// <summary>
        /// Gets the path to certificate password file to use for this endpoint.
        /// </summary>
        public string CertificatePasswordFile { get; set; }
    }
}
