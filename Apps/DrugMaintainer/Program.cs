// //-------------------------------------------------------------------------
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
    using HealthGateway.Common.FileDownload;    
    using HealthGateway.DrugMaintainer.Database;    
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration configuration = Initialize();

            // create service collection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, configuration);
        
            // create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();            

            // entry to run app
            serviceProvider.GetService<DrugMaintainerApp>().UpdateDrugProducts().Wait();
        }
        
        static IConfiguration Initialize()
        {
            string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine("Running in Environment {0}", environmentName);
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .Build();
        }        

        private static void ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            // add configured instance of logging
            serviceCollection.AddSingleton(new LoggerFactory());

            // add logging
            serviceCollection.AddLogging();

            // add httpclient
            serviceCollection.AddHttpClient();
            
            // add configuration
            serviceCollection.AddSingleton<IConfiguration>(configuration);

            // Add services
            serviceCollection.AddTransient<IDBContextFactory, DrugDBFactory>();
            serviceCollection.AddTransient<IDBContextFactory, DrugDBFactory>();
            serviceCollection.AddTransient<IFileDownloadService, FileDownloadService>();
            serviceCollection.AddTransient<IDrugProductParser, FederalDrugProductParser>();

            // Add app
            serviceCollection.AddTransient<DrugMaintainerApp>();
        }
    }
}
