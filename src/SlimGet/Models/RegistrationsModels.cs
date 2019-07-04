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
using Newtonsoft.Json;

namespace SlimGet.Models
{
    public sealed class RegistrationsLeafModel
    {
        [JsonProperty("@id", NullValueHandling = NullValueHandling.Include)]
        public string LeafUrl { get; set; }

        [JsonProperty("catalogEntry", NullValueHandling = NullValueHandling.Ignore)]
        public CatalogEntryModel CatalogEntry { get; set; }

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

    public sealed class CatalogEntryModel
    {
        // This is related to NuGet Catalog which is effectively a complete
        // historical record of what happened with the package, i.e. operations
        // such as publish, delete, list, unlist, etc.
        // As I do not want to implement the catalog, we'll be feeding it
        // registrations URL

        [JsonProperty("@id", NullValueHandling = NullValueHandling.Include)]
        public string CatalogUrl { get; set; }

        [JsonProperty("authors", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Authors { get; set; }

        [JsonProperty("dependencyGroups", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DependencyGroupModel> DependencyGroups { get; set; }

        [JsonProperty("deprecation", NullValueHandling = NullValueHandling.Ignore)]
        public DeprecationInfoModel Deprecation { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("iconUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string IconUrl { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Include)]
        public string Id { get; set; }

        [JsonProperty("licenseUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string LicenseUrl { get; set; }

        // ???????
        [JsonProperty("licenseExpression", NullValueHandling = NullValueHandling.Ignore)]
        public string LicenseExpression { get; set; }

        [JsonProperty("listed", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsListed { get; set; }

        [JsonProperty("minClientVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string MinimumClientVersion { get; set; }

        [JsonProperty("projectUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string ProjectUrl { get; set; }

        [JsonIgnore]
        public DateTimeOffset PublishedAt { get; set; }
        [JsonProperty("published", NullValueHandling = NullValueHandling.Ignore)]
        public string PublishedAtString => this.PublishedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

        [JsonProperty("requireLicenseAcceptance", NullValueHandling = NullValueHandling.Ignore)]
        public bool RequiresLicenseAcceptance { get; set; }

        [JsonProperty("summary", NullValueHandling = NullValueHandling.Ignore)]
        public string Summary { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Tags { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Include)]
        public string Version { get; set; }
    }

    public sealed class DependencyGroupModel
    {
        [JsonProperty("targetFramework", NullValueHandling = NullValueHandling.Ignore)]
        public string Framework { get; set; }

        [JsonProperty("dependencies", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DependencyModel> Dependencies { get; set; }
    }

    public sealed class DependencyModel
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Include)]
        public string Id { get; set; }

        [JsonProperty("range", NullValueHandling = NullValueHandling.Ignore)]
        public string VersionRange { get; set; }

        // You think the client is smart enough to figure this out on its own?
        [JsonProperty("registration", NullValueHandling = NullValueHandling.Ignore)]
        public string RegistrationUrl { get; set; }
    }

    public sealed class DeprecationInfoModel
    {
        [JsonIgnore]
        public IEnumerable<DeprecationReason> Reasons { get; set; }
        [JsonProperty("reasons", NullValueHandling = NullValueHandling.Include)]
        public IEnumerable<string> ReasonStrings => this.Reasons.Select(x => x.ToString());

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("alternatePackage", NullValueHandling = NullValueHandling.Ignore)]
        public DependencyModel AlternatePackage { get; set; }
    }

    public enum DeprecationReason
    {
        Legacy,
        CriticalBugs,
        Other
    }
}
