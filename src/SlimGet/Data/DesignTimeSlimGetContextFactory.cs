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
