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
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SlimGet.Filters
{
    public static class FlushGZipMiddleware
    {
        private static async Task FlushGZipAsync(HttpContext ctx, Func<Task> next)
        {
            await next().ConfigureAwait(false);

            if (ctx.Response.Body is GZipStream gz)
                await gz.FlushAsync().ConfigureAwait(false);
        }

        public static IApplicationBuilder UseFlushGZip(this IApplicationBuilder app)
            => app.Use(FlushGZipAsync);
    }
}
