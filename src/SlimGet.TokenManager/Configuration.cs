using Newtonsoft.Json;
using SlimGet.Data.Configuration;

namespace SlimGet.Tools
{
    public class Configuration
    {
        [JsonProperty("Storage")]
        public StorageConfiguration Storage { get; set; }

        [JsonProperty("Server")]
        public ServerConfiguration Server { get; set; }
    }

    public sealed class StorageConfiguration
    {
        [JsonProperty("PostgreSQL")]
        public PostgreSqlConfiguration PostgreSQL { get; set; }
    }

    public sealed class PostgreSqlConfiguration : IDatabaseConfiguration
    {
        [JsonProperty("Hostname")]
        public string Hostname { get; set; }

        [JsonProperty("Port")]
        public int Port { get; set; }

        [JsonProperty("Database")]
        public string Database { get; set; }

        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }

        [JsonProperty("UseSsl")]
        public bool UseSsl { get; set; }

        [JsonProperty("AlwaysTrustServerCertificate")]
        public bool AlwaysTrustServerCertificate { get; set; }
    }

    public sealed class ServerConfiguration : ITokenConfiguration
    {
        [JsonProperty("TokenHmacKey")]
        public string TokenHmacKey { get; set; }
    }
}
