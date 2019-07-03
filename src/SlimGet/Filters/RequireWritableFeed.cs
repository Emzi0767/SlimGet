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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;

namespace SlimGet.Filters
{
    public class RequireWritableFeed : IAuthorizationFilter
    {
        private PackageStorageConfiguration Configuration { get; }
        private ILogger<RequireWritableFeed> Logger { get; }

        public RequireWritableFeed(IOptions<StorageConfiguration> scfg, ILoggerFactory logger)
        {
            this.Configuration = scfg.Value.Packages;
            this.Logger = logger.CreateLogger<RequireWritableFeed>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (this.Configuration.ReadOnlyFeed)
            {
                this.Logger.LogError("Attempted writing to readonly feed");
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
