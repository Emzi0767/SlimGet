using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SlimGet.Data;
using SlimGet.Data.Configuration;
using StackExchange.Redis;

namespace SlimGet.Services
{
    public sealed class RedisService
    {
        private ConnectionMultiplexer Multiplexer { get; }
        private IDatabaseAsync Database { get; }
        private PackageKeyProvider KeyProvider { get; }

        public RedisService(IOptions<StorageConfiguration> scfg, PackageKeyProvider keyProvider)
        {
            this.KeyProvider = keyProvider;
            var rcfg = scfg.Value.Redis;
            this.Multiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { new DnsEndPoint(rcfg.Hostname, rcfg.Port) },
                ClientName = "SlimGet",
                DefaultDatabase = rcfg.Index,
                Password = rcfg.Password,
                Ssl = rcfg.UseSsl
            });
            this.Database = this.Multiplexer.GetDatabase();
        }

        public async Task SetPackageDownloadCountAsync(PackageInfo packageInfo, long count)
            => await this.Database.StringSetAsync(this.KeyProvider.GetPackageKey(packageInfo, KeyType.DownloadCount), count).ConfigureAwait(false);

        public async Task IncrementPackageDownloadCountAsync(PackageInfo packageInfo, long magnitude = 1)
            => await this.Database.StringIncrementAsync(this.KeyProvider.GetPackageKey(packageInfo, KeyType.DownloadCount), magnitude).ConfigureAwait(false);

        public async Task ClearPackageDownloadCountAsync(PackageInfo packageInfo)
            => await this.Database.KeyDeleteAsync(this.KeyProvider.GetPackageKey(packageInfo, KeyType.DownloadCount)).ConfigureAwait(false);

        public async Task<long> GetPackageDownloadCountAsync(PackageInfo packageInfo)
            => (long)await this.Database.StringGetAsync(this.KeyProvider.GetPackageKey(packageInfo, KeyType.DownloadCount)).ConfigureAwait(false);

        public async Task SetVersionDownloadCountAsync(PackageInfo packageInfo, long count)
            => await this.Database.StringSetAsync(this.KeyProvider.GetVersionKey(packageInfo, KeyType.DownloadCount), count).ConfigureAwait(false);

        public async Task IncrementVersionDownloadCountAsync(PackageInfo packageInfo, long magnitude = 1)
            => await this.Database.StringIncrementAsync(this.KeyProvider.GetVersionKey(packageInfo, KeyType.DownloadCount), magnitude).ConfigureAwait(false);

        public async Task ClearVersionDownloadCountAsync(PackageInfo packageInfo)
            => await this.Database.KeyDeleteAsync(this.KeyProvider.GetVersionKey(packageInfo, KeyType.DownloadCount)).ConfigureAwait(false);

        public async Task<long> GetVersionDownloadCountAsync(PackageInfo packageInfo)
            => (long)await this.Database.StringGetAsync(this.KeyProvider.GetVersionKey(packageInfo, KeyType.DownloadCount)).ConfigureAwait(false);
    }
}
