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
namespace HealthGateway.DBMaintainer
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;
    using HealthGateway.Database.Context;
    using HealthGateway.DBMaintainer.Apps;
    using HealthGateway.DBMaintainer.FileDownload;
    using HealthGateway.DBMaintainer.Parsers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Drug Loader console application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">The set of command line arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            IHost host = CreateWebHostBuilder(args).Build();

            // Process Federal file
            FedDrugDbApp? fedDrugApp = host.Services.GetService<FedDrugDbApp>();
            if (fedDrugApp != null)
            {
                await fedDrugApp.ProcessAsync("FedApprovedDatabase");
                await fedDrugApp.ProcessAsync("FedMarketedDatabase");
                await fedDrugApp.ProcessAsync("FedCancelledDatabase");
                await fedDrugApp.ProcessAsync("FedDormantDatabase");
            }
            else
            {
                Console.WriteLine("Federal Drug App is null");
            }

            // Process Provincial file
            BcpProvDrugDbApp? bcDrugApp = host.Services.GetService<BcpProvDrugDbApp>();
            if (bcDrugApp != null)
            {
                await bcDrugApp.ProcessAsync("PharmaCareDrugFile");
            }
            else
            {
                Console.WriteLine("Provincial Drug App is null");
            }
        }

        /// <summary>
        /// Creates the IHostBuilder for configuration, service injection etc.
        /// </summary>
        /// <returns>The IHostBuilder.</returns>
        /// <param name="args">The set of command line arguments.</param>
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required for migrations.")]
        public static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            return new HostBuilder()
                .ConfigureAppConfiguration(
                    (_, config) =>
                    {
                        config.SetBasePath(Directory.GetCurrentDirectory());
                        config.AddJsonFile("appsettings.json", true, true);
                        config.AddJsonFile($"appsettings.{environment}.json", true, true);
                        config.AddUserSecrets(typeof(Program).Assembly);
                        config.AddJsonFile("appsettings.local.json", true, true);
                    })
                .ConfigureServices(
                    (hostContext, services) =>
                    {
                        Console.WriteLine("Configuring Services...");
                        services.AddDbContextPool<GatewayDbContext>(
                            options => options.UseNpgsql(
                                hostContext.Configuration.GetConnectionString("GatewayConnection"),
                                x => x.MigrationsHistoryTable("__EFMigrationsHistory", "gateway")));

                        // Add HTTP Client
                        services.AddHttpClient();

                        // Add services
                        services.AddTransient<IFileDownloadService, FileDownloadService>();
                        services.AddTransient<IDrugProductParser, FederalDrugProductParser>();
                        services.AddTransient<IPharmaCareDrugParser, PharmaCareDrugParser>();
                        services.AddTransient<IPharmacyAssessmentParser, PharmacyAssessmentParser>();

                        // Add app
                        services.AddTransient<FedDrugDbApp>();
                        services.AddTransient<BcpProvDrugDbApp>();
                    })
                .ConfigureLogging(
                    logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                    });
        }
    }
}
