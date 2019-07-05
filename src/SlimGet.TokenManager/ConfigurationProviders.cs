using System.Text;
using SlimGet.Data.Configuration;
using SlimGet.Services;

namespace SlimGet.Tools
{
    public sealed class DatabaseConfigurationProvider : IDatabaseConfigurationProvider
    {
        private IDatabaseConfiguration DatabaseConfiguration { get; }

        public DatabaseConfigurationProvider(IDatabaseConfiguration dbconf)
        {
            this.DatabaseConfiguration = dbconf;
        }

        public IDatabaseConfiguration GetDatabaseConfiguration()
            => this.DatabaseConfiguration;
    }

    public sealed class TokenConfigurationProvider : ITokenConfigurationProvider
    {
        private ITokenConfiguration TokenConfiguration { get; }

        public TokenConfigurationProvider(ITokenConfiguration tkconf)
        {
            this.TokenConfiguration = tkconf;
        }

        public ITokenConfiguration GetTokenConfiguration()
            => this.TokenConfiguration;
    }

    public sealed class Utf8EncodingProvider : IEncodingProvider
    {
        public static UTF8Encoding UTF8 { get; } = new UTF8Encoding(false);

        public Encoding TextEncoding => UTF8;
    }
}
