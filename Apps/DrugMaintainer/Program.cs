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
    using System.Linq;
    using System.Collections.Generic;
    using HealthGateway.DIN.Models;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            IDrugProductParser parser = new FederalDrugProductParser();

            List<DrugProduct> drugProducts = parser.ParseDrugFile("/home/dev/Development/HealthGateway/Apps/DrugMaintainer/Resources/DrugProducts/drug.txt");
            List<ActiveIngredient> ingredients = parser.ParseActiveIngredientFile("/home/dev/Development/HealthGateway/Apps/DrugMaintainer/Resources/DrugProducts/ingred.txt", drugProducts);
            List<Company> companies = parser.ParseCompanyFile("/home/dev/Development/HealthGateway/Apps/DrugMaintainer/Resources/DrugProducts/comp.txt", drugProducts);
            List<Status> statuses = parser.ParseStatusFile("/home/dev/Development/HealthGateway/Apps/DrugMaintainer/Resources/DrugProducts/status.txt", drugProducts);
            List<Form> froms = parser.ParseFormFile("/home/dev/Development/HealthGateway/Apps/DrugMaintainer/Resources/DrugProducts/form.txt", drugProducts);
            List<Packaging> packagings = parser.ParsePackagingFile("/home/dev/Development/HealthGateway/Apps/DrugMaintainer/Resources/DrugProducts/package.txt", drugProducts);
            List<PharmaceuticalStd> pharmaceuticals = parser.ParsePharmaceuticalStdFile("/home/dev/Development/HealthGateway/Apps/DrugMaintainer/Resources/DrugProducts/pharm.txt", drugProducts);
            List<Route> routes = parser.ParseRouteFile("/home/dev/Development/HealthGateway/Apps/DrugMaintainer/Resources/DrugProducts/route.txt", drugProducts);
            List<Schedule> schedules = parser.ParseScheduleFile("/home/dev/Development/HealthGateway/Apps/DrugMaintainer/Resources/DrugProducts/schedule.txt", drugProducts);
            List<TherapeuticClass> therapeuticClasses = parser.ParseTherapeuticFile("/home/dev/Development/HealthGateway/Apps/DrugMaintainer/Resources/DrugProducts/ther.txt", drugProducts);
            List<VeterinarySpecies> veterinarySpecies = parser.ParseVeterinarySpeciesFile("/home/dev/Development/HealthGateway/Apps/DrugMaintainer/Resources/DrugProducts/vet.txt", drugProducts);


            var c = 3;
        }
    }
}
