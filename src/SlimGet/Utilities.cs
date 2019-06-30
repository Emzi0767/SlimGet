using System;
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

        public static Uri ToUri(this string s)
            => new Uri(s);
    }
}
