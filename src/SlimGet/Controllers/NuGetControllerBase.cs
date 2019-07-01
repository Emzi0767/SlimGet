using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    public abstract class NuGetControllerBase : Controller
    {
        protected SlimGetContext Database { get; }
        protected RedisService Redis { get; }
        protected FileSystemConfiguration FileSystemConfiguration { get; }
        protected PackageStorageConfiguration PackageStorageConfiguration { get; }

        public NuGetControllerBase(SlimGetContext db, RedisService redis, IOptions<StorageConfiguration> storcfg)
        {
            var cfg = storcfg.Value;

            this.Database = db;
            this.Redis = redis;
            this.FileSystemConfiguration = cfg.FileSystem;
            this.PackageStorageConfiguration = cfg.Packages;
        }
    }
}
