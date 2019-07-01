using Microsoft.AspNetCore.Mvc.Filters;

namespace SlimGet.Filters
{
    public sealed class ServerHeaderAppender : IAlwaysRunResultFilter
    {
        private static string HeaderContent { get; } = $"SlimGet/{Utilities.VersionString}";

        public void OnResultExecuted(ResultExecutedContext context)
        { }

        public void OnResultExecuting(ResultExecutingContext context)
            => context.HttpContext.Response.Headers.Add("X-Powered-By", HeaderContent);
    }
}
