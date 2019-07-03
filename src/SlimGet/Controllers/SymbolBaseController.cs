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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimGet.Data;
using SlimGet.Data.Configuration;
using SlimGet.Filters;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [Route("/api/v3/symbolstore"), ApiController, AllowAnonymous, ServiceFilter(typeof(RequireSymbolsEnabled))]
    public sealed class SymbolBaseController : NuGetControllerBase
    {
        public SymbolBaseController(SlimGetContext db, RedisService redis, IFileSystemService fs, IOptions<StorageConfiguration> storcfg, ILoggerFactory logger)
            : base(db, redis, fs, storcfg, logger)
        { }

        [Route(""), HttpGet]
        public IActionResult Dummy() => this.NoContent();

        [Route("index2.txt"), HttpGet]
        public IActionResult Index2()
            => this.Content("", "text/plain", Utilities.UTF8);

        [Route("pingme.txt"), HttpGet]
        public IActionResult PingMe()
            => this.Content("", "text/plain", Utilities.UTF8);

        [Route("{file}/{sig}/{file2}"), Route("{t2prefix}/{file}/{sig}/{file2}"), HttpGet]
        public async Task<IActionResult> Symbols(string file, string sig, string file2, CancellationToken cancellationToken)
        {
            if (file2 != file)
                return this.NotFound();

            if (sig.Length != 33 && sig.Length != 40)
                return this.BadRequest();

            if (!Guid.TryParseExact(sig.AsSpan(0, 32), "N", out var id))
                return this.BadRequest();

            if (!int.TryParse(sig.AsSpan(32), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var age))
                return this.BadRequest();

            var symbols = await this.Database.PackageSymbols
                .Include(x => x.Binary)
                .ThenInclude(x => x.Package)
                .FirstOrDefaultAsync(x => x.Identifier == id && x.Age == age, cancellationToken).ConfigureAwait(false);
            if (symbols == null)
                return this.NotFound();

            if (symbols.Name != file)
                return this.NotFound();

            var pdb = this.FileSystem.OpenSymbolsRead(new PackageInfo(symbols.PackageId, symbols.Binary.Package.NuGetVersion), id, age);
            return this.File(pdb, "application/octet-stream", symbols.Name);
        }
    }
}
