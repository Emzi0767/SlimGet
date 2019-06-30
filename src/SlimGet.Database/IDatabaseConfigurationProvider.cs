using SlimGet.Data.Configuration;

namespace SlimGet.Services
{
    public interface IDatabaseConfigurationProvider
    {
        IDatabaseConfiguration GetDatabaseConfiguration();
    }
}
