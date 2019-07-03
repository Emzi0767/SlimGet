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
