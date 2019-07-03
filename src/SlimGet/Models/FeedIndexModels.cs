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
using Newtonsoft.Json;

namespace SlimGet.Models
{
    public sealed class FeedResourceModel
    {
        /// <summary>
        /// Gets the base URL of the resource.
        /// </summary>
        [JsonProperty("@id")]
        public Uri BaseUrl { get; }

        /// <summary>
        /// Gets the type of the resource.
        /// </summary>
        [JsonProperty("@type")]
        public string Type { get; }

        /// <summary>
        /// Gets the human-readable comment for the resource.
        /// </summary>
        [JsonProperty("comment")]
        public string Comment { get; }

        public FeedResourceModel(Uri baseUrl, string type, string comment)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentNullException(nameof(type), "Resource type cannot be null or empty.");

            this.BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl), "Resource base URL cannot be null.");
            this.Type = type;
            this.Comment = !string.IsNullOrWhiteSpace(comment) ? comment : string.Empty;
        }
    }

    public sealed class FeedIndexModel
    {
        /// <summary>
        /// Gets the version of the feed index.
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; }

        /// <summary>
        /// Gets the services available in the feed.
        /// </summary>
        [JsonProperty("resources")]
        public IEnumerable<FeedResourceModel> Resources { get; }

        // @context

        public FeedIndexModel(string version, IEnumerable<FeedResourceModel> resources)
        {
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentNullException(nameof(version), "Version cannot be null or empty.");

            this.Version = version;
            this.Resources = resources ?? throw new ArgumentException(nameof(resources), "Resource collection cannot be null.");
        }
    }
}
