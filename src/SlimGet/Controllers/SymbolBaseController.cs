using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [Route("/api/v3/symbolstore"), ApiController, AllowAnonymous]
    public sealed class SymbolBaseController : NuGetControllerBase
    {
        public SymbolBaseController(SlimGetContext db, RedisService redis, IFileSystemService fs, IOptions<StorageConfiguration> storcfg)
            : base(db, redis, fs, storcfg)
        { }

        [Route(""), HttpGet]
        public IActionResult Dummy() => this.NoContent();

        [Route("index2.txt"), HttpGet]
        public IActionResult Index2()
            => this.Content("", "text/plain", Utilities.UTF8);

        [Route("pingme.txt"), HttpGet]
        public IActionResult PingMe()
            => this.Content("", "text/plain", Utilities.UTF8);

        [Route("{file}/{sig}/{file2}"), Route("{t2prefix}/{file}/{sig}/{file2}"), HttpGet]
        public IActionResult Symbols(string file, string sig, string file2)
            => this.NoContent();
    }
}
