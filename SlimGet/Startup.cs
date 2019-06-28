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

            services.AddSingleton<ConnectionStringProvider>()
                .AddDbContext<SlimGetContext>(ServiceLifetime.Transient)
                .AddSingleton<RedisService>();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseExceptionHandler("/error");
            app.UseStatusCodePages(this.RenderStatusCodeAsync);

            if (!env.IsDevelopment())
                app.UseHsts();

            app.UseStaticFiles();
            app.UseMvc(routes => { });

            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<SlimGetContext>())
                db.Database.Migrate();
        }

        private async Task RenderStatusCodeAsync(StatusCodeContext ctx)
        {
            var json = JsonConvert.SerializeObject(new SimpleErrorModel(Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier), Formatting.Indented);

            ctx.HttpContext.Response.ContentType = "application/json";
            await ctx.HttpContext.Response.WriteAsync(json, Utilities.UTF8);
        }
    }
}
