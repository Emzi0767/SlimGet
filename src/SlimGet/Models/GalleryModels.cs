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
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace SlimGet.Models
{
    public sealed class GalleryAboutModel
    {
        public Uri NuGetFeedUrl { get; }
        public Uri SymbolsUrl { get; }
        public Uri SymbolsPushUrl { get; }
        public bool SymbolsEnabled { get; }
        public bool IsWritable { get; }

        public GalleryAboutModel(Uri feedUrl, Uri symbolsUrl, Uri symbolsPushUrl, bool symbolsEnabled, bool writble)
        {
            this.NuGetFeedUrl = feedUrl;
            this.SymbolsUrl = symbolsUrl;
            this.SymbolsPushUrl = symbolsPushUrl;
            this.SymbolsEnabled = symbolsEnabled;
            this.IsWritable = writble;
        }
    }

    public sealed class GalleryIndexModel
    {
        public int PackageCount { get; }
        public int VersionCount { get; }

        public GalleryIndexModel(int pkgCount, int verCount)
        {
            this.PackageCount = pkgCount;
            this.VersionCount = verCount;
        }
    }

    public sealed class GallerySearchModel
    {
        [FromQuery(Name = "q"), Display(Name = "Search query"), StringLength(32767, ErrorMessage = "Package ID query is too long.")]
        public string Query { get; set; }

        [FromQuery(Name = "skip"), Range(0, int.MaxValue, ErrorMessage = "Skip amount cannot be less than 0.")]
        public int Skip { get; set; } = 0;

        [FromQuery(Name = "pre"), Display(Name = "Include prerelease versions")]
        public bool Prerelease { get; set; } = false;
    }

    public sealed class GalleryPackageListItemModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string IconUrl { get; set; }
        public IEnumerable<string> Authors { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public long DownloadCount { get; set; }
        public DateTimeOffset PublishedAt { get; set; }
        public DateTimeOffset LastUpdatedAt { get; set; }
        public NuGetVersion LatestVersion { get; set; }
        public string Description { get; set; }
    }

    public sealed class GallerySearchListModel
    {
        public int TotalCount { get; }
        public IEnumerable<GalleryPackageListItemModel> Items { get; }
        public int NextPage { get; }
        public int PreviousPage { get; }
        public string Query => this.SearchQuery?.Query;
        public bool IncludePrerelease => this.SearchQuery.Prerelease;
        public GallerySearchModel SearchQuery { get; }

        public GallerySearchListModel(int total, IEnumerable<GalleryPackageListItemModel> items, int next, int prev, GallerySearchModel searchQuery)
        {
            this.TotalCount = total;
            this.Items = items;
            this.NextPage = next;
            this.PreviousPage = prev;
            this.SearchQuery = searchQuery;
        }
    }

    public sealed class GalleryPackageInfoModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string IconUrl { get; set; }
        public string ProjectUrl { get; set; }
        public string LicenseUrl { get; set; }
        public string RepositoryUrl { get; set; }
        public IEnumerable<string> Authors { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public long DownloadCount { get; set; }
        public long VersionDownloadCount { get; set; }
        public DateTimeOffset PublishedAt { get; set; }
        public NuGetVersion Version { get; set; }
        public string Description { get; set; }
        public string DownloadUrl { get; set; }
        public string ManifestUrl { get; set; }
        //public string SymbolsUrl { get; set; }
        public IEnumerable<GalleryPackageDependencyGroupModel> DependencyGroups { get; set; }
        public string OwnerId { get; set; }
        public IEnumerable<(string version, NuGetVersion nugetVersion, long downloads, DateTimeOffset publishedAt)> AllVersions { get; set; }
    }

    public sealed class GalleryPackageDependencyGroupModel
    {
        public NuGetFramework Framework { get; set; }
        public IEnumerable<GalleryPackageDependencyModel> Dependencies { get; set; }
    }

    public sealed class GalleryPackageDependencyModel
    {
        public string Id { get; set; }
        public NuGetVersion MinVersion { get; set; }
        public NuGetVersion MaxVersion { get; set; }
        public bool MinInclusive { get; set; }
        public bool MaxInclusive { get; set; }
    }
}
