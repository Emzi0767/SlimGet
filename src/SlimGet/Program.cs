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
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;

namespace SlimGet
{
    public class Program
    {
        public static void Main(string[] args)
            => CreateWebHostBuilder(args).Build().Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
            => WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(kopts =>
                {
                    var kcfg = kopts.ApplicationServices.GetService<IOptions<ServerConfiguration>>().Value;
                    kopts.AddServerHeader = false;

                    if (kcfg != null && kcfg.Listen != null && kcfg.Listen.Length > 0)
                    {
                        var scfg = kcfg.SslCertificate;
                        var lcfg = kcfg.Listen;

                        var cpwd = "";
                        using (var fs = File.OpenRead(scfg.PasswordFile))
                        using (var sr = new StreamReader(fs, Utilities.UTF8))
                            cpwd = sr.ReadToEnd();

                        var cert = new X509Certificate2(scfg.Location, cpwd);
                        foreach (var endpoint in lcfg)
                        {
                            if (endpoint.UseSsl && (kcfg == null || scfg == null || string.IsNullOrWhiteSpace(scfg.Location) || string.IsNullOrWhiteSpace(scfg.PasswordFile)))
                                continue;

                            kopts.Listen(new IPEndPoint(IPAddress.Parse(endpoint.IpAddress), endpoint.Port), lopts =>
                            {
                                if (endpoint.UseSsl)
                                {
                                    lopts.Protocols = HttpProtocols.Http1AndHttp2;
                                    lopts.UseHttps(cert, sopts => sopts.SslProtocols = SslProtocols.Tls12);
                                }
                                else
                                {
                                    lopts.Protocols = HttpProtocols.Http1;
                                }
                            });
                        }
                    }
                    else
                    {
                        kopts.Listen(new IPEndPoint(IPAddress.Any, 5000), lopts => lopts.Protocols = HttpProtocols.Http1);
                    }
                });
    }
}
