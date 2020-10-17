// This file is a part of SlimGet project.
//
// Copyright 2019 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

        public RedisService(IOptions<CacheConfiguration> cacheOpts, PackageKeyProvider keyProvider)
        {
            this.KeyProvider = keyProvider;
            var rcfg = cacheOpts.Value;
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
