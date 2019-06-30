namespace SlimGet.Data.Configuration
{
    public interface IDatabaseConfiguration
    {
        string Hostname { get; }
        int Port { get; }
        string Database { get; }
        string Username { get; }
        string Password { get; }
        bool UseSsl { get; }
        bool AlwaysTrustServerCertificate { get; }
    }
}
