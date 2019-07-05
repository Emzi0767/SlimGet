using SlimGet.Data.Configuration;

namespace SlimGet.Services
{
    public interface ITokenConfigurationProvider
    {
        ITokenConfiguration GetTokenConfiguration();
    }
}
