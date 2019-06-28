using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SlimGet.Models;

namespace SlimGet.Controllers
{
    public class ErrorController : Controller
    {
        private IHostingEnvironment Environment { get; }

        public ErrorController(IHostingEnvironment env)
        {
            this.Environment = env;
        }

        [Route("/"), AllowAnonymous]
        public IActionResult Index()
            => this.Content("ok", "text/plain", Utilities.UTF8);

        [Route("/error"), AllowAnonymous, ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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