namespace SlimGet.Data.Configuration
{
    public sealed class DatabaseConfiguration
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseSsl { get; set; }
        public bool AlwaysTrustServerCertificate { get; set; }
    }
}
