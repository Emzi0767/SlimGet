using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;

namespace SlimGet.Filters
{
    public class RequireWritableFeed : IAuthorizationFilter
    {
        private PackageStorageConfiguration Configuration { get; }
        private ILogger<RequireWritableFeed> Logger { get; }

        public RequireWritableFeed(IOptions<StorageConfiguration> scfg, ILoggerFactory logger)
        {
            this.Configuration = scfg.Value.Packages;
            this.Logger = logger.CreateLogger<RequireWritableFeed>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (this.Configuration.ReadOnlyFeed)
            {
                this.Logger.LogError("Attempted writing to readonly feed");
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
