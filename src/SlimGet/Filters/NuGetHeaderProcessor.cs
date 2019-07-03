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
