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
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SlimGet
{
    public static class Utilities
    {
        public static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);
        public static string VersionString { get; }

        static Utilities()
        {
            var a = Assembly.GetExecutingAssembly();

            var aiv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            VersionString = aiv != null ? aiv.InformationalVersion : a.GetName().Version.ToString();
        }

        public static string AbsoluteUrl(this IUrlHelper url, string action, string controller, HttpContext ctx)
            => url.AbsoluteUrl(action, controller, ctx, null);

        public static string AbsoluteUrl(this IUrlHelper url, string action, string controller, HttpContext ctx, object @params)
            => url.Action(action, controller, @params, ctx.Request.Scheme);

        public static string AbsoluteUrl(this IUrlHelper url, string routeName, HttpContext ctx)
            => url.AbsoluteUrl(routeName, ctx, null);

        public static string AbsoluteUrl(this IUrlHelper url, string routeName, HttpContext ctx, object @params)
            => url.RouteUrl(routeName, @params, ctx.Request.Scheme);

        public static Uri ToUri(this string s)
            => new Uri(s);

        public static string MakeRelativeTo(this FileSystemInfo fi, DirectoryInfo dir)
        {
            var fifn = fi.FullName;
            var dirfn = dir.FullName;
            if (!dirfn.EndsWith(Path.DirectorySeparatorChar) && !dirfn.EndsWith(Path.DirectorySeparatorChar))
                dirfn += Path.DirectorySeparatorChar;

            var full = new Uri(fifn, UriKind.Absolute);
            var root = new Uri(dirfn, UriKind.Absolute);

            return root.MakeRelativeUri(full).ToString();
        }
    }
}
