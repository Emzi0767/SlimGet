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
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Design;
using SlimGet.Data.Configuration;
using SlimGet.Services;

namespace SlimGet.Data
{
    public sealed class DesignTimeSlimGetContextFactory : IDesignTimeDbContextFactory<SlimGetContext>
    {
        public SlimGetContext CreateDbContext(string[] args)
            => new SlimGetContext(
                ConnectionStringProvider.Create(
                    new DesignTimeDatabaseConfigurationProvider().GetDatabaseConfiguration()));
    }

    public sealed class DesignTimeDatabaseConfigurationProvider
    {
        public DatabaseConfiguration GetDatabaseConfiguration()
        {
            var json = "{}";
            using (var fs = File.OpenRead("slimget.json"))
            using (var sr = new StreamReader(fs, AbstractionUtilities.UTF8))
                json = sr.ReadToEnd();

            return JsonSerializer.Deserialize<SlimGetConfiguration>(json).Storage.Database;
        }

        private sealed class SlimGetConfiguration
        {
            [JsonPropertyName("Storage")]
            public StorageConfiguration Storage { get; set; }
        }
    }
}
