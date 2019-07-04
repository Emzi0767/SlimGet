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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;
using SlimGet.Models;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [SlimGetRoute(Routing.GalleryRouteName), AllowAnonymous]
    public class GalleryController : Controller
    {
        private PackageStorageConfiguration PackageStorageConfiguration { get; }
        private SlimGetContext Database { get; }

        public GalleryController(IOptions<StorageConfiguration> scfg, SlimGetContext db)
        {
            this.PackageStorageConfiguration = scfg.Value.Packages;
            this.Database = db;
        }

        [HttpGet, SlimGetRoute(Routing.GalleryIndexRouteName), SlimGetRoute(Routing.InheritRoute)]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var pkgCount = await this.Database.Packages.CountAsync(cancellationToken).ConfigureAwait(false);
            var verCount = await this.Database.PackageVersions.CountAsync(cancellationToken).ConfigureAwait(false);

            return this.View(new GalleryIndexModel(pkgCount, verCount));
        }

        [HttpGet, SlimGetRoute(Routing.GalleryPackageIndexRouteName)]
        public IActionResult Packages()
            => this.View();

        [HttpGet, SlimGetRoute(Routing.GalleryPackageDetailsRouteName)]
        public IActionResult Packages(string id)
            => this.View();

        [HttpGet, SlimGetRoute(Routing.GalleryPackageVersionRouteName)]
        public IActionResult Packages(string id, string version)
            => this.View();

        [HttpGet, SlimGetRoute(Routing.GalleryAboutRouteName)]
        public IActionResult About()
        {
            var feedUrl = this.Url.AbsoluteUrl(Routing.FeedIndexRouteName, this.HttpContext);
            var symbolUrl = this.Url.AbsoluteUrl(Routing.DownloadSymbolsRouteName, this.HttpContext);
            var symbolPushUrl = this.Url.AbsoluteUrl(Routing.PublishSymbolsRouteName, this.HttpContext);

            return this.View(new GalleryAboutModel(
                new Uri(feedUrl),
                new Uri(symbolUrl),
                new Uri(symbolPushUrl),
                this.PackageStorageConfiguration.SymbolsEnabled,
                !this.PackageStorageConfiguration.ReadOnlyFeed));
        }
    }
}
