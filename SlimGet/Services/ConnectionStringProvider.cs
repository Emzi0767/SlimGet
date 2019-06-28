using Microsoft.Extensions.Options;
using Npgsql;
using SlimGet.Data.Configuration;

namespace SlimGet.Services
{
    public sealed class ConnectionStringProvider
    {
        public string ConnectionString { get; }

        public ConnectionStringProvider(IOptions<StorageConfiguration> storageConfig)
        {
            var dbc = storageConfig.Value.PostgreSQL;
            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = dbc.Hostname,
                Port = dbc.Port,
                Database = dbc.Database,
                Username = dbc.Username,
                Password = dbc.Password,
                SslMode = dbc.UseSsl ? SslMode.Require : SslMode.Disable,
                TrustServerCertificate = dbc.UseSsl && dbc.AlwaysTrustServerCertificate
            };

            this.ConnectionString = csb.ConnectionString;
        }
    }
}
