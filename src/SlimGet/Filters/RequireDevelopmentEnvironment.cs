using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace SlimGet.Filters
{
    public class RequireDevelopmentEnvironment : IAuthorizationFilter
    {
        private IHostingEnvironment Environment { get; }
        private ILogger<RequireDevelopmentEnvironment> Logger { get; }

        public RequireDevelopmentEnvironment(IHostingEnvironment env, ILoggerFactory logger)
        {
            this.Environment = env;
            this.Logger = logger.CreateLogger<RequireDevelopmentEnvironment>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!this.Environment.IsDevelopment())
            {
                this.Logger.LogError("Attempted to access development endpoint in non-development environment");
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
