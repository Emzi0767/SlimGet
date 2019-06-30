using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;

namespace SlimGet.Services
{
    public sealed class DatabaseConfigurationProvider : IDatabaseConfigurationProvider
    {
        private DatabaseConfiguration DatabaseConfiguration { get; }

        public DatabaseConfigurationProvider(IOptions<StorageConfiguration> storageConfig)
        {
            this.DatabaseConfiguration = storageConfig.Value.PostgreSQL;
        }

        public IDatabaseConfiguration GetDatabaseConfiguration()
            => this.DatabaseConfiguration;
    }
}
