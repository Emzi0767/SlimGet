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

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;
using SlimGet.Models;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [SlimGetRoute(Routing.SearchRouteName), ApiController, AllowAnonymous]
    public class SearchController : NuGetControllerBase
    {
        public SearchController(SlimGetContext db, RedisService redis, IFileSystemService fs, IOptions<StorageConfiguration> storcfg, ILoggerFactory logger)
            : base(db, redis, fs, storcfg, logger)
        { }

        [SlimGetRoute(Routing.SearchQueryRouteName), HttpGet]
        public async Task<IActionResult> Search(SearchQueryModel search, CancellationToken cancellationToken)
            => this.NoContent();

        [SlimGetRoute(Routing.SearchAutocompleteRouteName), HttpGet]
        public async Task<IActionResult> Autocomplete(SearchQueryModel search, CancellationToken cancellationToken)
            => this.NoContent();

        [SlimGetRoute(Routing.SearchAutocompleteRouteName), HttpGet]
        public async Task<IActionResult> Enumerate(SearchEnumerateModel enumerate, CancellationToken cancellationToken)
            => this.NoContent();
    }
}
