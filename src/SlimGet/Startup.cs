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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SlimGet.Data.Configuration;
using SlimGet.Filters;
using SlimGet.Models;
using SlimGet.Services;

namespace SlimGet
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            var cfg = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddJsonFile("slimget.json", false)
                .Build();

            this.Configuration = cfg;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .Configure<StorageConfiguration>(opts => this.Configuration.GetSection("Storage").Bind(opts))
                .Configure<ServerConfiguration>(opts => this.Configuration.GetSection("Server").Bind(opts));

            services.AddHsts(opts =>
            {
                opts.Preload = true;
                opts.IncludeSubDomains = true;
                opts.MaxAge = TimeSpan.FromDays(365);
            });

            services.AddSingleton<IEncodingProvider, EncodingProvider>()
                .AddSingleton<ITokenConfigurationProvider, TokenConfigurationProvider>()
                .AddSingleton<IDatabaseConfigurationProvider, DatabaseConfigurationProvider>()
                .AddSingleton<ConnectionStringProvider>()
                .AddSingleton<PackageKeyProvider>()
                .AddDbContext<SlimGetContext>(ServiceLifetime.Transient)
                .AddSingleton<RedisService>()
                .AddSingleton<TokenService>()
                .AddSingleton<IFileSystemService, FileSystemService>()
                .AddSingleton<PackageProcessingService>()
                .AddSingleton<RequireDevelopmentEnvironment>()
                .AddSingleton<RequireWritableFeed>()
                .AddSingleton<RequireSymbolsEnabled>();

            services.AddAuthentication(AuthenticationSchemeSelector.AuthenticationSchemeName)
                .AddPolicyScheme(AuthenticationSchemeSelector.AuthenticationSchemeName, AuthenticationSchemeSelector.AuthenticationSchemeName, AuthenticationSchemeSelector.ConfigureSelector)
                .AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(TokenAuthenticationHandler.AuthenticationSchemeName, null)
                .AddScheme<BypassAuthenticationOptions, BypassAuthenticationHandler>(BypassAuthenticationHandler.AuthenticationSchemeName, null);

            services.AddMvc(mvcopts =>
            {
                mvcopts.Filters.Add(new ServerHeaderAppender());
                mvcopts.Filters.Add(new NuGetHeaderProcessor());

                mvcopts.InputFormatters.Add(new RawTextBodyFormatter());
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseExceptionHandler("/error")
                .UseStatusCodePages(this.RenderStatusCodeAsync)
                .UseStaticFiles()
                .UseAuthentication()
                .UseFlushGZip()
                .UseMvc(routes => { });

            if (!env.IsDevelopment())
                app.UseHsts();

            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<SlimGetContext>())
                db.Database.Migrate();
        }

        private async Task RenderStatusCodeAsync(StatusCodeContext ctx)
        {
            var json = JsonConvert.SerializeObject(new SimpleErrorModel(Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier), Formatting.Indented);

            ctx.HttpContext.Response.ContentType = "application/json";
            await ctx.HttpContext.Response.WriteAsync(json, Utilities.UTF8).ConfigureAwait(false);
        }
    }
}
