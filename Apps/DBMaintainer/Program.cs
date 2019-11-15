﻿// //-------------------------------------------------------------------------
// // Copyright © 2019 Province of British Columbia
// //
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //-------------------------------------------------------------------------
namespace HealthGateway.DrugMaintainer
{
    using System;
    using System.IO;
    using HealthGateway.DrugMaintainer.Apps;
    using HealthGateway.Database.Context;
    using HealthGateway.Common.FileDownload;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    class Program
    {
        static void Main(string[] args)
        {
            IHost host = CreateWebHostBuilder(args).Build();
            FedDrugDBApp fedDrugApp = host.Services.GetService<FedDrugDBApp>();
            fedDrugApp.Process("FedApprovedDatabase");
            fedDrugApp.Process("FedMarketedDatabase");
            fedDrugApp.Process("FedCancelledDatabase");
            fedDrugApp.Process("FedDormantDatabase");
            
            BCPProvDrugDBApp bcDrugApp = host.Services.GetService<BCPProvDrugDBApp>();
            bcDrugApp.Process("PharmaCareDrugFile");
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return new HostBuilder()
                       .ConfigureAppConfiguration((hostingContext, config) =>
                       {
                           config.SetBasePath(Directory.GetCurrentDirectory());
                           config.AddJsonFile($"appsettings.json", true, true);
                           config.AddJsonFile($"appsettings.{environment}.json", true, true);
                       })
                       .ConfigureServices((hostContext, services) =>
                       {
                           Console.WriteLine("Configuring Services...");
                           services.AddDbContextPool<GatewayDbContext>(options =>
                                options.UseNpgsql(hostContext.Configuration.GetConnectionString("GatewayConnection")));

                           // Add HTTP Client
                           services.AddHttpClient();

                           // Add services
                           services.AddTransient<IFileDownloadService, FileDownloadService>();
                           services.AddTransient<IDrugProductParser, FederalDrugProductParser>();
                           services.AddTransient<IPharmaCareDrugParser, PharmaCareDrugParser>();

                           // Add app
                           services.AddTransient<FedDrugDBApp>();
                           services.AddTransient<BCPProvDrugDBApp>();
                       })
                       .ConfigureLogging(logging =>
                       {
                           logging.ClearProviders();
                           logging.AddConsole();
                       });
        }
    }
}
