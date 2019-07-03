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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;
using SlimGet.Models;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [ApiController, AllowAnonymous]
    public sealed class IndexController : NuGetControllerBase
    {
        public IndexController(SlimGetContext db, RedisService redis, IFileSystemService fs, IOptions<StorageConfiguration> storcfg, ILoggerFactory logger)
            : base(db, redis, fs, storcfg, logger)
        { }

        [Route("/api/v3/index.json"), HttpGet]
        public IActionResult Index()
        {
            var packagePublish = this.CreateResourceModels(this.Url.AbsoluteUrl("Push", "PackagePublish", this.HttpContext).ToUri(), "PackagePublish", "2.0.0");
            var symbolPackagePublish = this.CreateResourceModels(this.Url.AbsoluteUrl("Push", "SymbolPublish", this.HttpContext).ToUri(), "SymbolPackagePublish", "4.9.0");
            var searchQuery = this.CreateResourceModels(this.Url.AbsoluteUrl("Search", "Search", this.HttpContext).ToUri(), "SearchQueryService", "", "3.0.0-beta", "3.0.0-rc");
            var searchAutocomplete = this.CreateResourceModels(this.Url.AbsoluteUrl("Autocomplete", "Search", this.HttpContext).ToUri(), "SearchAutocompleteService", "", "3.0.0-beta", "3.0.0-rc");
            var registrationsBase = this.CreateResourceModels(this.Url.AbsoluteUrl("Dummy", "RegistrationsBase", this.HttpContext).ToUri(), "RegistrationsBase", "", "3.0.0-beta", "3.0.0-rc");
            var registrationsBaseGz = this.CreateResourceModels(this.Url.AbsoluteUrl("DummyGz", "RegistrationsBase", this.HttpContext).ToUri(), "RegistrationsBase", "3.4.0");
            var registrationsBaseSemVer2 = this.CreateResourceModels(this.Url.AbsoluteUrl("DummySemVer", "RegistrationsBase", this.HttpContext).ToUri(), "RegistrationsBase", "3.6.0");
            var packageDetailsUriTemplate = this.CreateResourceModels(this.Url.AbsoluteUrl("Packages", "Gallery", this.HttpContext, new { id = "{id}", version = "{version}" }).Replace("%7B", "{").Replace("%7D", "}").ToUri(), "PackageDetailsUriTemplate", "5.1.0");
            var packageBaseAddress = this.CreateResourceModels(this.Url.AbsoluteUrl("Dummy", "PackageBase", this.HttpContext).ToUri(), "PackageBaseAddress", "3.0.0");

            var resources = packagePublish
                .Concat(symbolPackagePublish)
                .Concat(searchQuery)
                .Concat(searchAutocomplete)
                .Concat(registrationsBase)
                .Concat(registrationsBaseGz)
                .Concat(registrationsBaseSemVer2)
                .Concat(packageDetailsUriTemplate)
                .Concat(packageBaseAddress);

            var index = new FeedIndexModel("3.0.0", resources);
            return this.Json(index);
        }

        private IEnumerable<FeedResourceModel> CreateResourceModels(Uri baseUrl, string type, params string[] versions)
        {
            foreach (var version in versions)
            {
                var xtype = !string.IsNullOrWhiteSpace(type) ? $"{type}/{version}" : type;
                yield return new FeedResourceModel(baseUrl, xtype, null);
            }
        }
    }
}
