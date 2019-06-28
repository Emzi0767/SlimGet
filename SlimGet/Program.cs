using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(kopts =>
                {
                    kopts.AddServerHeader = false;
                    kopts.Listen(new IPEndPoint(IPAddress.Any, 5000), lopts =>
                    {
                        var scfg = lopts.ApplicationServices.GetService<IOptions<ServerConfiguration>>().Value.SslCertificate;

                        if (scfg == null || string.IsNullOrWhiteSpace(scfg.Location) || string.IsNullOrWhiteSpace(scfg.PasswordFile))
                        {
                            lopts.Protocols = HttpProtocols.Http1;
                        }
                        else
                        {
                            var cpwd = "";
                            using (var fs = File.OpenRead(scfg.PasswordFile))
                            using (var sr = new StreamReader(fs, Utilities.UTF8))
                                cpwd = sr.ReadToEnd();

                            var cert = new X509Certificate2(scfg.Location, cpwd);

                            lopts.Protocols = HttpProtocols.Http1AndHttp2;
                            lopts.UseHttps(cert, sopts => sopts.SslProtocols = SslProtocols.Tls12);
                        }
                    });
                });
    }
}
