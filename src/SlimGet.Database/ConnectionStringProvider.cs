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

using Microsoft.Extensions.Options;
using Npgsql;
using SlimGet.Data.Configuration;

namespace SlimGet.Services
{
    public sealed class ConnectionStringProvider
    {
        public string ConnectionString { get; }

        public ConnectionStringProvider(IOptions<DatabaseConfiguration> cfg)
            : this(cfg.Value)
        { }

        private ConnectionStringProvider(DatabaseConfiguration dbc)
        {
            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = dbc.Hostname,
                Port = dbc.Port,
                Database = dbc.Database,
                Username = dbc.Username,
                Password = dbc.Password,
                SslMode = dbc.UseSsl ? SslMode.Require : SslMode.Disable,
                TrustServerCertificate = dbc.UseSsl && dbc.TrustServerCertificate
            };

            this.ConnectionString = csb.ConnectionString;
        }

        public static ConnectionStringProvider Create(DatabaseConfiguration dbc)
            => new ConnectionStringProvider(dbc);
    }
}
