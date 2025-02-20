﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stratis.FederatedSidechains.AdminDashboard.Hubs;
using Stratis.FederatedSidechains.AdminDashboard.HostedServices;
using Stratis.FederatedSidechains.AdminDashboard.Settings;
using Stratis.FederatedSidechains.AdminDashboard.Services;
using Microsoft.Extensions.Hosting;

namespace Stratis.FederatedSidechains.AdminDashboard
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {            
            IConfigurationSection defaultEndpoints = this.Configuration.GetSection("DefaultEndpoints");
            var defaultEndpointsSettings = new DefaultEndpointsSettings
            {
                StratisNode = !string.IsNullOrEmpty(this.Configuration["mainchainport"]) ? $"http://localhost:{this.Configuration["mainchainport"]}" : defaultEndpoints["StratisNode"],
                EnvType = defaultEndpoints["EnvType"],
                SidechainNodeType = this.Configuration["nodetype"] ?? defaultEndpoints["SidechainNodeType"],
                SidechainNode = !string.IsNullOrEmpty(this.Configuration["sidechainport"]) ? $"http://localhost:{this.Configuration["sidechainport"]}" : defaultEndpoints["SidechainNode"],
                IntervalTime = defaultEndpoints["IntervalTime"]
            };

            if (!string.IsNullOrEmpty(this.Configuration["nodetype"]))
            {
                defaultEndpointsSettings.SidechainNodeType =
                    this.Configuration["nodetype"].Contains("50", StringComparison.OrdinalIgnoreCase) || this.Configuration["nodetype"]
                        .Contains("fifty", StringComparison.OrdinalIgnoreCase)
                        ? NodeTypes.FiftyK
                        : NodeTypes.TenK;
            }

            if (!string.IsNullOrEmpty(this.Configuration["env"]))
            {
                defaultEndpointsSettings.EnvType = this.Configuration["env"].Contains("testnet", StringComparison.OrdinalIgnoreCase)
                    ? NodeEnv.TestNet
                    : NodeEnv.MainNet;
            }

            services.AddSingleton(defaultEndpointsSettings);
            services.AddDistributedMemoryCache();

            services.AddTransient<ApiRequester>();

            services.AddHostedService<FetchingBackgroundService>();

            services.AddMvc();
            services.AddSignalR();
           
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var cachePeriod = Environment.IsDevelopment() ? "600" : "604800";
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
                }
            });            
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<DataUpdaterHub>("/ws-updater");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
