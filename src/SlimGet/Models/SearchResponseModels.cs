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

using System.Collections.Generic;
using System.Text.Json.Serialization;
using SlimGet.Attributes;
using SlimGet.Data;

namespace SlimGet.Models
{
    [JsonOverrideConverter]
    public sealed class SearchResponseModel
    {
        [JsonPropertyName("totalHits")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public int TotalResultCount { get; set; }

        [JsonPropertyName("data")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public IEnumerable<SearchResultModel> ResultPage { get; set; }
    }

    [JsonOverrideConverter]
    public sealed class SearchResultModel
    {
        [JsonPropertyName("id")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public string Id { get; set; }

        [JsonPropertyName("version")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public string Version { get; set; }

        [JsonPropertyName("description")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string Description { get; set; }

        [JsonPropertyName("versions")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public IEnumerable<SearchVersionModel> Versions { get; set; }

        [JsonPropertyName("authors")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public IEnumerable<string> Authors { get; set; }

        [JsonPropertyName("iconUrl")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string IconUrl { get; set; }

        [JsonPropertyName("licenseUrl")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string LicenseUrl { get; set; }

        [JsonPropertyName("owners")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public IEnumerable<string> Owners { get; set; }

        [JsonPropertyName("projectUrl")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string ProjectUrl { get; set; }

        [JsonPropertyName("registration")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string RegistrationUrl { get; set; }

        [JsonPropertyName("summary")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string Summary { get; set; }

        [JsonPropertyName("tags")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public IEnumerable<string> Tags { get; set; }

        [JsonPropertyName("title")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string Title { get; set; }

        [JsonPropertyName("totalDownloads")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public long DownloadCount { get; set; }

        [JsonPropertyName("verified")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public bool? IsVerified { get; set; } = false;
    }

    [JsonOverrideConverter]
    public sealed class SearchVersionModel
    {
        [JsonPropertyName("@id")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public string RegistrationLeafUrl { get; set; }

        [JsonPropertyName("version")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string Version { get; set; }

        [JsonPropertyName("downloads")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public long DownloadCount { get; set; }
    }

    [JsonOverrideConverter]
    public sealed class SearchAutocompleteResponseModel
    {
        [JsonPropertyName("totalHits")]
        public int TotalResultCount { get; set; }

        [JsonPropertyName("data")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public IEnumerable<string> Results { get; set; }
    }

    [JsonOverrideConverter]
    public sealed class SearchEnumerateResponseModel
    {
        [JsonPropertyName("data")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public IEnumerable<string> Versions { get; set; }
    }
}
