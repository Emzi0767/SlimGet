using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SlimGet.Models;

namespace SlimGet.Controllers
{
    [AllowAnonymous]
    public class MetaController : Controller
    {
        private IHostingEnvironment Environment { get; }

        public MetaController(IHostingEnvironment env)
        {
            this.Environment = env;
        }

        [Route("/test/{rc?}/{ra?}"), ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Test(string rc = "Feed", string ra = "Index")
        {
            return this.Environment.IsDevelopment()
                ? this.Content(this.Url.AbsoluteUrl(ra, rc, this.HttpContext), "text/plain", Utilities.UTF8)
                : this.Unauthorized() as IActionResult;
        }

        [Route("/error"), ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var jsonopts = new JsonSerializerSettings { Formatting = Formatting.Indented };

            this.Response.StatusCode = 500;
            if (!this.Environment.IsDevelopment())
                return this.Json(new SimpleErrorModel(Activity.Current?.Id ?? this.HttpContext.TraceIdentifier), jsonopts);

            var exHandler = this.HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            return this.Json(new DeveloperErrorModel(Activity.Current?.Id ?? this.HttpContext.TraceIdentifier, exHandler?.Path, exHandler?.Error), jsonopts);
        }
    }
}
