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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;

namespace SlimGet
{
    public class Program
    {
        public static void Main(string[] args)
            => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .UseStartup<Startup>()
                    .UseKestrel(kopts =>
                    {
                        kopts.AddServerHeader = false;
                        var kcfg = kopts.ApplicationServices.GetService<IOptions<HttpConfiguration>>().Value;

                        if (kcfg == null || kcfg.Listen == null || kcfg.Listen.Length <= 0)
                        {
                            kopts.Listen(new IPEndPoint(IPAddress.Any, 5000), lopts => lopts.Protocols = HttpProtocols.Http1);
                            return;
                        }

                        kopts.Limits.MaxRequestBodySize = kcfg.MaxRequestSizeBytes;
                        foreach (var endpoint in kcfg.Listen)
                        {
                            if (endpoint.UseSsl &&
                                (string.IsNullOrWhiteSpace(endpoint.CertificateFile) ||
                                !File.Exists(endpoint.CertificateFile) ||
                                string.IsNullOrWhiteSpace(endpoint.CertificatePasswordFile) ||
                                !File.Exists(endpoint.CertificatePasswordFile)))
                                continue;

                            if (endpoint.UseSsl)
                            {
                                var cff = new FileInfo(endpoint.CertificateFile);
                                var cpf = new FileInfo(endpoint.CertificatePasswordFile);

                                var cert = new byte[cff.Length];
                                var cpwd = "";

                                using (var fsc = cff.OpenRead())
                                    fsc.Read(cert, 0, cert.Length);

                                using (var fsp = cpf.OpenRead())
                                using (var sr = new StreamReader(fsp, AbstractionUtilities.UTF8))
                                    cpwd = sr.ReadToEnd();

                                var x509 = new X509Certificate2(cert, cpwd);

                                kopts.Listen(new IPEndPoint(IPAddress.Parse(endpoint.Address), endpoint.Port), lopts =>
                                {
                                    lopts.Protocols = HttpProtocols.Http1AndHttp2;
                                    lopts.UseHttps(x509, sopts => sopts.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13);
                                });
                            }
                            else
                            {
                                kopts.Listen(new IPEndPoint(IPAddress.Parse(endpoint.Address), endpoint.Port), lopts => lopts.Protocols = HttpProtocols.Http1);
                            }
                        }
                    }));
    }
}
