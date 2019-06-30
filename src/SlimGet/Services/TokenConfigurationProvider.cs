using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;

namespace SlimGet.Services
{
    public class TokenConfigurationProvider : ITokenConfigurationProvider
    {
        private ServerConfiguration ServerConfiguration { get; }

        public TokenConfigurationProvider(IOptions<ServerConfiguration> serverConfig)
        {
            this.ServerConfiguration = serverConfig.Value;
        }

        public ITokenConfiguration GetTokenConfiguration()
            => this.ServerConfiguration;
    }
}
