using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SlimGet.Controllers
{
    [AllowAnonymous]
    public class GalleryController : Controller
    {
        [Route("/v3/index.json"), HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}