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
    public sealed class RegistrationsLeafModel
    {
        [JsonProperty("@id", NullValueHandling = NullValueHandling.Include)]
        public string LeafUrl { get; set; }

        // CatalogEntry

        [JsonProperty("listed")]
        public bool IsListed { get; set; }

        [JsonProperty("packageContent", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentUrl { get; set; }

        [JsonIgnore]
        public DateTimeOffset PublishedAt { get; set; }
        [JsonProperty("published")]
        public string PublishedAtString => this.PublishedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

        [JsonProperty("registration", NullValueHandling = NullValueHandling.Ignore)]
        public string RegistrationIndexUrl { get; set; }
    }

    public sealed class RegistrationsPageModel
    {
        [JsonProperty("@id", NullValueHandling = NullValueHandling.Include)]
        public string PageUrl { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<RegistrationsLeafModel> Items { get; set; }

        [JsonProperty("lower", NullValueHandling = NullValueHandling.Include)]
        public string MinVersion { get; set; }

        [JsonProperty("upper", NullValueHandling = NullValueHandling.Include)]
        public string MaxVersion { get; set; }

        [JsonProperty("parent", NullValueHandling = NullValueHandling.Ignore)]
        public string IndexUrl { get; set; }
    }

    public sealed class RegistrationsIndexModel
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Include)]
        public IEnumerable<RegistrationsPageModel> Pages { get; set; }
    }
}
