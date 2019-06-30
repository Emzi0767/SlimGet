using Microsoft.AspNetCore.Mvc.Filters;

namespace SlimGet
{
    public sealed class ServerHeaderResponseFilter : IAlwaysRunResultFilter
    {
        private static string HeaderContent { get; } = $"SlimGet/{Utilities.VersionString}";

        public void OnResultExecuted(ResultExecutedContext context)
        { }

        public void OnResultExecuting(ResultExecutingContext context)
            => context.HttpContext.Response.Headers.Add("X-Powered-By", HeaderContent);
    }
}
