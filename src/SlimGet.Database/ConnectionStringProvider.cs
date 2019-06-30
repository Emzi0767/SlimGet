using Npgsql;

namespace SlimGet.Services
{
    public sealed class ConnectionStringProvider
    {
        public string ConnectionString { get; }

        public ConnectionStringProvider(IDatabaseConfigurationProvider dbcfgProvider)
        {
            var dbc = dbcfgProvider.GetDatabaseConfiguration();
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
