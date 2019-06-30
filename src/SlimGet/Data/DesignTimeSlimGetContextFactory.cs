using System.IO;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json;
using SlimGet.Data.Configuration;
using SlimGet.Services;

namespace SlimGet.Data
{
    public sealed class DesignTimeSlimGetContextFactory : IDesignTimeDbContextFactory<SlimGetContext>
    {
        public SlimGetContext CreateDbContext(string[] args)
            => new SlimGetContext(new ConnectionStringProvider(new DesignTimeDatabaseConfigurationProvider()));
    }

    public sealed class DesignTimeDatabaseConfigurationProvider : IDatabaseConfigurationProvider
    {
        public IDatabaseConfiguration GetDatabaseConfiguration()
        {
            var json = "{}";
            using (var fs = File.OpenRead("slimget.json"))
            using (var sr = new StreamReader(fs, Utilities.UTF8))
                json = sr.ReadToEnd();

            return JsonConvert.DeserializeObject<SlimGetConfiguration>(json).Storage.PostgreSQL;
        }

        private sealed class SlimGetConfiguration
        {
            [JsonProperty("Storage")]
            public StorageConfiguration Storage { get; set; }
        }
    }
}
