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

using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SlimGet.Data;
using SlimGet.Data.Configuration;
using SlimGet.Filters;
using SlimGet.Services;

namespace SlimGet
{
    public sealed class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            // Configuration loading
            // Given an object structure like
            // CONFIG:
            // prop: bool
            // listen: endpoint[]
            // 
            // ENDPOINT:
            // address: string
            // port: int
            //
            // Array indexes are treated as keys
            // To set /prop via:
            // - envvars: SLIMGET__PROP=true (note double underscore)
            // - cmdline: --prop=false
            //
            // To set /listen/0/* via:
            // - envvars (note double underscores):
            //   SLIMGET__LISTEN__0__ADDRESS=0.0.0.0
            //   SLIMGET__LISTEN__0__PORT=5000
            // - cmdline:
            //   --listen:0:address=127.0.0.1
            //   --listen:0:port=5000
            //
            // Supported configuration sources, in order of precedence (first is lowest priority - meaning higher 
            // priority will override its values):
            // 1. appsettings.json
            // 2. appsettings.*.json (* is env, i.e. development, production, or staging)
            // 3. *.json (specified via SLIMGET__CONFIGURATION; defaults to slimget.json)
            // 4. Environment variables
            // 5. Command line

            // For explanation on L80 and L81, see
            // https://github.com/dotnet/runtime/issues/40911
            // Load envvars and cmdline switches
            var cfg = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                //.AddEnvironmentVariables("SLIMGET__")
                .AddEnvironmentVariables("SLIMGET:")
                .AddCommandLine(Environment.GetCommandLineArgs())
                .Build();

            // Load file config
            var cfgf = new ConfigurationBuilder()
                .AddJsonFile(cfg.GetSection("Configuration")?.Value ?? "slimget.json", optional: true)
                .Build();

            // Prepend file config so it has lower priority
            this.Configuration = new ConfigurationBuilder()
                .AddConfiguration(cfgf)
                .AddConfiguration(cfg)
                .Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // HSTS
            services.AddHsts(opts =>
            {
                opts.Preload = true;
                opts.IncludeSubDomains = true;
                opts.MaxAge = TimeSpan.FromDays(365);
            });

            // Configuration
            services.AddOptions<SlimGetConfiguration>()
                .Bind(this.Configuration)
                .ValidateDataAnnotations();

            services.AddOptions<StorageConfiguration>()
                .Bind(this.Configuration.GetSection("Storage"))
                .ValidateDataAnnotations();

            services.AddOptions<DatabaseConfiguration>()
                .Bind(this.Configuration.GetSection("Storage:Database"))
                .ValidateDataAnnotations();

            services.AddOptions<CacheConfiguration>()
                .Bind(this.Configuration.GetSection("Storage:Cache"))
                .ValidateDataAnnotations();

            services.AddOptions<BlobStorageConfiguration>()
                .Bind(this.Configuration.GetSection("Storage:Blobs"))
                .ValidateDataAnnotations();

            services.AddOptions<PackageStorageConfiguration>()
                .Bind(this.Configuration.GetSection("Storage:Packages"))
                .ValidateDataAnnotations();

            services.AddOptions<HttpConfiguration>()
                .Bind(this.Configuration.GetSection("Http"))
                .ValidateDataAnnotations();

            services.AddOptions<HttpProxyConfiguration>()
                .Bind(this.Configuration.GetSection("Http:Proxy"))
                .ValidateDataAnnotations();

            services.AddOptions<SecurityConfiguration>()
                .Bind(this.Configuration.GetSection("Security"))
                .ValidateDataAnnotations();

            // Other services
            services.AddSingleton<ConnectionStringProvider>()
                .AddSingleton<PackageKeyProvider>()
                .AddDbContext<SlimGetContext>(ServiceLifetime.Transient)
                .AddSingleton<RedisService>()
                .AddSingleton<TokenService>()
                .AddSingleton<IFileSystemService, FileSystemService>()
                .AddSingleton<PackageProcessingService>()
                .AddSingleton<RequireDevelopmentEnvironment>()
                .AddSingleton<RequireWritableFeed>()
                .AddSingleton<RequireSymbolsEnabled>()
                .AddSingleton<DownloadCountTransferService>();

            // Authentication/authorization
            services.AddAuthentication(AuthenticationHandlerSelector.AuthenticationSchemeName)
                .AddPolicyScheme(
                    AuthenticationHandlerSelector.AuthenticationSchemeName,
                    AuthenticationHandlerSelector.AuthenticationSchemeName,
                    AuthenticationHandlerSelector.ConfigureSelector)
                .AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(TokenAuthenticationHandler.AuthenticationSchemeName, null)
                .AddScheme<BypassAuthenticationOptions, BypassAuthenticationHandler>(BypassAuthenticationHandler.AuthenticationSchemeName, null);

            // MVC
            services
                .AddControllersWithViews(mvcopts =>
                {
                    mvcopts.Filters.Add(new ServerHeaderAppender());
                    mvcopts.Filters.Add(new NuGetHeaderProcessor());

                    mvcopts.InputFormatters.Add(new RawTextBodyFormatter());
                })
                .AddJsonOptions(o =>
                {
                    var jsonOpts = o.JsonSerializerOptions;

                    jsonOpts.Converters.Add(new NullHandlingJsonConverterFactory());

                    jsonOpts.IgnoreNullValues = true;
                    jsonOpts.IgnoreReadOnlyProperties = false;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IOptions<HttpProxyConfiguration> httpProxyOpts)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(this.HandlePipelineException)
                    .UseStatusCodePages(this.RenderStatusCode)
                    .UseHsts();
            }

            var httpProxy = httpProxyOpts.Value;
            var forwardConfig = new ForwardedHeadersOptions
            {
                ForwardedHeaders = httpProxy.Enable ? ForwardedHeaders.All : ForwardedHeaders.None,
                ForwardLimit = httpProxy.Limit
            };
            if (httpProxy.Enable)
                foreach (var scidr in httpProxy.Networks)
                {
                    var cidr = scidr.AsSpan();
                    var ix = cidr.IndexOf('/');
                    var ip = IPAddress.Parse(cidr.Slice(0, ix));
                    var sz = cidr.Slice(ix + 1).ParseAsInt();

                    forwardConfig.KnownNetworks.Add(new IPNetwork(ip, sz));
                }

            // Typically, this will not run on HTTPS, HTTP will be used in staging for debugging
            //app.UseHttpsRedirection()
            app.UseForwardedHeaders(forwardConfig)
                .UseStaticFiles()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseFlushGZip()
                .UseEndpoints(endpoints => endpoints.MapControllers());
        }

        private Task RenderStatusCode(StatusCodeContext ctx)
            => this.RunHandlerAsync(ctx.HttpContext);

        private void HandlePipelineException(IApplicationBuilder app)
            => app.Run(this.RunHandlerAsync);

        private async Task RunHandlerAsync(HttpContext ctx)
        {
            ctx.Response.ContentType = "application/json";

            SimpleErrorModel error;
            var reqid = Activity.Current.Id ?? ctx.TraceIdentifier;
            var env = ctx.RequestServices.GetService<IWebHostEnvironment>();
            var exhpf = ctx.Features.Get<IExceptionHandlerPathFeature>();
            if (env.IsDevelopment() && exhpf?.Error != null)
            {
                ctx.Response.StatusCode = 500;
                error = new DeveloperErrorModel(reqid, ctx.Request.Path, exhpf.Error);
            }
            else
            {
                error = new SimpleErrorModel(reqid);
            }

            await JsonSerializer.SerializeAsync(ctx.Response.Body, error, AbstractionUtilities.DefaultJsonOptions);
        }
    }
}
