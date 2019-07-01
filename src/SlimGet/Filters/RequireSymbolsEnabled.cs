using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;

namespace SlimGet.Filters
{
    public class RequireSymbolsEnabled : IResourceFilter
    {
        private PackageStorageConfiguration Configuration { get; }
        private ILogger<RequireSymbolsEnabled> Logger { get; }

        public RequireSymbolsEnabled(IOptions<StorageConfiguration> scfg, ILoggerFactory logger)
        {
            this.Configuration = scfg.Value.Packages;
            this.Logger = logger.CreateLogger<RequireSymbolsEnabled>();
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        { }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (!this.Configuration.SymbolsEnabled)
            {
                this.Logger.LogError("Attempted access to symbol storage when symbols are disabled");
                context.Result = new NotFoundResult();
            }
        }
    }
}
