using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SlimGet.Filters
{
    public sealed class NuGetHeaderProcessor : IResourceFilter
    {
        public void OnResourceExecuted(ResourceExecutedContext context)
        { }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var headers = context.HttpContext.Request.Headers;
            var routeData = context.RouteData.Values;

            if (headers.TryGetValue("X-NuGet-Client-Version", out var clientVersion) && clientVersion.Count > 0)
                routeData["NuGet-ProtocolVersion"] = clientVersion.First();

            if (headers.TryGetValue("X-NuGet-Protocol-Version", out var protoVersion) && protoVersion.Count > 0)
                routeData["NuGet-ProtocolVersion"] = protoVersion.First();

            if (headers.TryGetValue("X-NuGet-Session-Id", out var sessionId) && protoVersion.Count > 0)
                routeData["NuGet-SessionId"] = sessionId.First();
        }
    }
}
