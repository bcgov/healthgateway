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
    using System.Collections.Generic;
    using HealthGateway.DIN.Models;
    using HealthGateway.Common.FileDownload;
    using Microsoft.Extensions.Logging;

    public class Maintainer
    {
        private IDrugProductParser parser;
        private IFileDownloadService downloadService;

        public Maintainer(IDrugProductParser parser, IFileDownloadService downloadService)
        {
            this.parser = parser;
            this.downloadService = downloadService;
        }

        public void DownloadFiles()
        {
            /*ILogger logger = ILoggerFactory.;
            FileDownloadService downloader = new FileDownloadService();*/
        }

        public void ParseFiles()
        {
            List<DrugProduct> drugProducts = this.parser.ParseDrugFile("drug.txt");
            List<ActiveIngredient> ingredients = this.parser.ParseActiveIngredientFile("ingred.txt", drugProducts);
            List<Company> companies = this.parser.ParseCompanyFile("comp.txt", drugProducts);
            List<Status> statuses = this.parser.ParseStatusFile("status.txt", drugProducts);
            List<Form> froms = this.parser.ParseFormFile("form.txt", drugProducts);
            List<Packaging> packagings = this.parser.ParsePackagingFile("package.txt", drugProducts);
            List<PharmaceuticalStd> pharmaceuticals = this.parser.ParsePharmaceuticalStdFile("pharm.txt", drugProducts);
            List<Route> routes = this.parser.ParseRouteFile("route.txt", drugProducts);
            List<Schedule> schedules = this.parser.ParseScheduleFile("schedule.txt", drugProducts);
            List<TherapeuticClass> therapeuticClasses = this.parser.ParseTherapeuticFile("ther.txt", drugProducts);
            List<VeterinarySpecies> veterinarySpecies = this.parser.ParseVeterinarySpeciesFile("vet.txt", drugProducts);

            var c = 3;
        }
    }
}
