using System.Net;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;
using StackExchange.Redis;

namespace SlimGet.Services
{
    public sealed class RedisService
    {
        private ConnectionMultiplexer Connections { get; }

        public RedisService(IOptions<StorageConfiguration> scfg)
        {
            var rcfg = scfg.Value.Redis;
            this.Connections = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { new DnsEndPoint(rcfg.Hostname, rcfg.Port) },
                ClientName = "SlimGet",
                DefaultDatabase = rcfg.Index,
                Password = rcfg.Password,
                Ssl = rcfg.UseSsl
            });
        }
    }
}
