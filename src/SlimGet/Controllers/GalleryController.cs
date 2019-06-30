using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlimGet.Models;

namespace SlimGet.Controllers
{
    [Route("/[controller]/[action]"), AllowAnonymous]
    public class GalleryController : Controller
    {
        [HttpGet, Route("/"), Route("/[controller]")]
        public IActionResult Index() => this.View();

        [HttpGet]
        public IActionResult Packages() => this.View();

        [HttpGet, Route("{id}")]
        public IActionResult Packages(string id) => this.View();

        [HttpGet, Route("{id}/{version}")]
        public IActionResult Packages(string id, string version) => this.View();

        [HttpGet]
        public IActionResult About()
        {
            var feedUrl = this.Url.AbsoluteUrl("Index", "Feed", this.HttpContext);

            return this.View(new GalleryAboutModel(new Uri(feedUrl)));
        }
    }
}
