//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Hangfire
{
    using System;
    using System.Diagnostics.Contracts;
    using global::Hangfire;
    using global::Hangfire.PostgreSql;
    using HealthGateway.Common.AspNetConfiguration;
    using HealthGateway.Common.FileDownload;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using HealthGateway.DrugMaintainer;
    using HealthGateway.DrugMaintainer.Apps;
    using HealthGateway.Hangfire.Jobs;
    using Healthgateway.Hangfire.Utils;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The startup class.
    /// </summary>
    public class Startup
    {
        private readonly StartupConfiguration startupConfig;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The injected Environment provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="logger">The injected logger provider.</param>
        public Startup(IHostingEnvironment env, IConfiguration configuration, ILogger<Startup> logger)
        {
            this.startupConfig = new StartupConfiguration(configuration, env, logger);
            this.configuration = configuration;
            this.logger = logger;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The passed in Service Collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.startupConfig.ConfigureHttpServices(services);

            services.AddDbContextPool<GatewayDbContext>(options =>
                options.UseNpgsql(
                    this.configuration.GetConnectionString("GatewayConnection"),
                    b => b.MigrationsAssembly(nameof(Database))));

            services.AddTransient<IEmailDelegate, DBEmailDelegate>();
            services.AddTransient<IEmailJob, EmailJob>();

            // DB Maintainer Services
            services.AddTransient<IFileDownloadService, FileDownloadService>();
            services.AddTransient<IDrugProductParser, FederalDrugProductParser>();
            services.AddTransient<IPharmaCareDrugParser, PharmaCareDrugParser>();

            // Add app
            services.AddTransient<FedDrugDBApp>();
            services.AddTransient<BCPProvDrugDBApp>();

            // Enable Hangfire
            services.AddHangfire(x => x.UsePostgreSqlStorage(this.configuration.GetConnectionString("GatewayConnection")));
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The passed in Application Builder.</param>
        /// <param name="env">The passed in Environment.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Contract.Requires(env != null);
            this.logger.LogInformation($"Hosting Environment: {env.EnvironmentName}");
            app.UseHangfireDashboard();
            app.UseHangfireServer();

            // Schedule Health Gateway Jobs
            BackgroundJob.Enqueue<DBMigrationsJob>(j => j.Process());
            SchedulerHelper.ScheduleJob<IEmailJob>(this.configuration, "SendLowPriorityEmail", j => j.SendLowPriorityEmails());
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugDBApp>(this.configuration, "FedApprovedDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugDBApp>(this.configuration, "FedMarketedDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugDBApp>(this.configuration, "FedCancelledDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<FedDrugDBApp>(this.configuration, "FedDormantDatabase");
            SchedulerHelper.ScheduleDrugLoadJob<BCPProvDrugDBApp>(this.configuration, "PharmaCareDrugFile");

            this.startupConfig.UseForwardHeaders(app);
            this.startupConfig.UseHttp(app);
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = (content) =>
                {
                    var headers = content.Context.Response.Headers;
                    var contentType = headers["Content-Type"];
                    if (contentType != "application/x-gzip" && !content.File.Name.EndsWith(".gz", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return;
                    }

                    var mimeTypeProvider = new FileExtensionContentTypeProvider();
                    var fileNameToTry = content.File.Name.Substring(0, content.File.Name.Length - 3);
                    if (mimeTypeProvider.TryGetContentType(fileNameToTry, out var mimeType))
                    {
                        headers.Add("Content-Encoding", "gzip");
                        headers["Content-Type"] = mimeType;
                    }
                },
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
