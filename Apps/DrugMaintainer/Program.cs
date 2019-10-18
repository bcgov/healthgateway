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
    using HealthGateway.Common.FileDownload;

    using System.IO;
    using Microsoft.Extensions.Configuration;


    class Program
    {
        private static IConfiguration configuration;
        
        static void Initialize()
        {
            string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine("Running in Environment {0}", environmentName);
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .Build();
        }
        static void Main(string[] args)
        {
            Initialize();

            Console.WriteLine("DIN Parsing...");
            IDrugProductParser parser = new FederalDrugProductParser("/home/dev/Development/HealthGateway/Apps/DrugMaintainer/Resources/DrugProducts/");
            IFileDownloadService downloader = new FileDownloadService();

            Maintainer maintainer = new Maintainer(parser, downloader);
            maintainer.ParseFiles();

            Console.WriteLine("Adding Entities to DB");
            using (var ctx = new DrugDBContext(configuration))
            {
                ctx.DrugProduct.AddRange(drugProducts);
                ctx.ActiveIngredient.AddRange(ingredients);
                ctx.Company.AddRange(companies);
                ctx.Status.AddRange(statuses);
                ctx.Form.AddRange(forms);
                ctx.Packaging.AddRange(packagings);
                ctx.PharmaceuticalStd.AddRange(pharmaceuticals);
                ctx.Route.AddRange(routes);
                ctx.Schedule.AddRange(schedules);
                ctx.TherapeuticClass.AddRange(therapeuticClasses);
                ctx.VeterinarySpecies.AddRange(veterinarySpecies);
                Console.WriteLine("Saving Entities");
                ctx.SaveChanges();
            }
        }
    }
}
