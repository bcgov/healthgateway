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
    using System.Collections.Generic;
    using DrugMaintainer.Database;
    using HealthGateway.DIN.Models;
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
            IDrugProductParser parser = new FederalDrugProductParser();

            List<DrugProduct> drugProducts = parser.ParseDrugFile("./Resources/DrugProducts/drug.txt");
            List<ActiveIngredient> ingredients = parser.ParseActiveIngredientFile("./Resources/DrugProducts/ingred.txt", drugProducts);
            List<Company> companies = parser.ParseCompanyFile("./Resources/DrugProducts/comp.txt", drugProducts);
            List<Status> statuses = parser.ParseStatusFile("./Resources/DrugProducts/status.txt", drugProducts);
            List<Form> forms = parser.ParseFormFile("./Resources/DrugProducts/form.txt", drugProducts);
            List<Packaging> packagings = parser.ParsePackagingFile("./Resources/DrugProducts/package.txt", drugProducts);
            List<PharmaceuticalStd> pharmaceuticals = parser.ParsePharmaceuticalStdFile("./Resources/DrugProducts/pharm.txt", drugProducts);
            List<Route> routes = parser.ParseRouteFile("./Resources/DrugProducts/route.txt", drugProducts);
            List<Schedule> schedules = parser.ParseScheduleFile("./Resources/DrugProducts/schedule.txt", drugProducts);
            List<TherapeuticClass> therapeuticClasses = parser.ParseTherapeuticFile("./Resources/DrugProducts/ther.txt", drugProducts);
            List<VeterinarySpecies> veterinarySpecies = parser.ParseVeterinarySpeciesFile("./Resources/DrugProducts/vet.txt", drugProducts);

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
