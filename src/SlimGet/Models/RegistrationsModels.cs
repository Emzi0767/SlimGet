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
using System.Linq;
using System.Text.Json.Serialization;
using SlimGet.Attributes;
using SlimGet.Data;

namespace SlimGet.Models
{
    [JsonOverrideConverter]
    public sealed class RegistrationsLeafModel
    {
        [JsonPropertyName("@id")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public string LeafUrl { get; set; }

        [JsonPropertyName("catalogEntry")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public CatalogEntryModel CatalogEntry { get; set; }

        [JsonPropertyName("listed")]
        public bool IsListed { get; set; }

        [JsonPropertyName("packageContent")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string ContentUrl { get; set; }

        [JsonIgnore]
        public DateTimeOffset PublishedAt { get; set; }
        [JsonPropertyName("published")]
        public string PublishedAtString => this.PublishedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

        [JsonPropertyName("registration")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string RegistrationIndexUrl { get; set; }
    }

    [JsonOverrideConverter]
    public sealed class RegistrationsPageModel
    {
        [JsonPropertyName("@id")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public string PageUrl { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public IEnumerable<RegistrationsLeafModel> Items { get; set; }

        [JsonPropertyName("lower")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public string MinVersion { get; set; }

        [JsonPropertyName("upper")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public string MaxVersion { get; set; }

        [JsonPropertyName("parent")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string IndexUrl { get; set; }
    }

    [JsonOverrideConverter]
    public sealed class RegistrationsIndexModel
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public IEnumerable<RegistrationsPageModel> Pages { get; set; }
    }

    [JsonOverrideConverter]
    public sealed class CatalogEntryModel
    {
        // This is related to NuGet Catalog which is effectively a complete
        // historical record of what happened with the package, i.e. operations
        // such as publish, delete, list, unlist, etc.
        // As I do not want to implement the catalog, we'll be feeding it
        // registrations URL

        [JsonPropertyName("@id")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public string CatalogUrl { get; set; }

        [JsonPropertyName("authors")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public IEnumerable<string> Authors { get; set; }

        [JsonPropertyName("dependencyGroups")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public IEnumerable<DependencyGroupModel> DependencyGroups { get; set; }

        [JsonPropertyName("deprecation")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public DeprecationInfoModel Deprecation { get; set; }

        [JsonPropertyName("description")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string Description { get; set; }

        [JsonPropertyName("iconUrl")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string IconUrl { get; set; }

        [JsonPropertyName("id")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public string Id { get; set; }

        [JsonPropertyName("licenseUrl")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string LicenseUrl { get; set; }

        // SPDX license id
        // https://spdx.org/licenses/
        [JsonPropertyName("licenseExpression")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string LicenseExpression { get; set; }

        [JsonPropertyName("listed")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public bool IsListed { get; set; }

        [JsonPropertyName("minClientVersion")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string MinimumClientVersion { get; set; }

        [JsonPropertyName("projectUrl")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string ProjectUrl { get; set; }

        [JsonIgnore]
        public DateTimeOffset PublishedAt { get; set; }
        [JsonPropertyName("published")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string PublishedAtString => this.PublishedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

        [JsonPropertyName("requireLicenseAcceptance")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public bool RequiresLicenseAcceptance { get; set; }

        [JsonPropertyName("summary")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string Summary { get; set; }

        [JsonPropertyName("tags")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public IEnumerable<string> Tags { get; set; }

        [JsonPropertyName("title")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string Title { get; set; }

        [JsonPropertyName("version")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public string Version { get; set; }
    }

    [JsonOverrideConverter]
    public sealed class DependencyGroupModel
    {
        [JsonPropertyName("targetFramework")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string Framework { get; set; }

        [JsonPropertyName("dependencies")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public IEnumerable<DependencyModel> Dependencies { get; set; }
    }

    [JsonOverrideConverter]
    public sealed class DependencyModel
    {
        [JsonPropertyName("id")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public string Id { get; set; }

        [JsonPropertyName("range")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string VersionRange { get; set; }

        // You think the client is smart enough to figure this out on its own?
        [JsonPropertyName("registration")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string RegistrationUrl { get; set; }
    }

    [JsonOverrideConverter]
    public sealed class DeprecationInfoModel
    {
        [JsonIgnore]
        public IEnumerable<DeprecationReason> Reasons { get; set; }
        [JsonPropertyName("reasons")]
        [JsonNullHandling(JsonNullHandling.Include)]
        public IEnumerable<string> ReasonStrings => this.Reasons.Select(x => x.ToString());

        [JsonPropertyName("message")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public string Message { get; set; }

        [JsonPropertyName("alternatePackage")]
        [JsonNullHandling(JsonNullHandling.Ignore)]
        public DependencyModel AlternatePackage { get; set; }
    }

    public enum DeprecationReason
    {
        Legacy,
        CriticalBugs,
        Other
    }
}
