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
    using HealthGateway.Common.Database.Models;
    using System.Collections.Generic;

    public interface IDrugProductParser
    {
        List<DrugProduct> ParseDrugFile(string filePath);
        List<ActiveIngredient> ParseActiveIngredientFile(string filePath, IEnumerable<DrugProduct> drugProducts);
        List<Company> ParseCompanyFile(string filePath, IEnumerable<DrugProduct> drugProducts);
        List<Status> ParseStatusFile(string filePath, IEnumerable<DrugProduct> drugProducts);
        List<Form> ParseFormFile(string filePath, IEnumerable<DrugProduct> drugProducts);
        List<Packaging> ParsePackagingFile(string filePath, IEnumerable<DrugProduct> drugProducts);
        List<PharmaceuticalStd> ParsePharmaceuticalStdFile(string filePath, IEnumerable<DrugProduct> drugProducts);
        List<Route> ParseRouteFile(string filePath, IEnumerable<DrugProduct> drugProducts);
        List<Schedule> ParseScheduleFile(string filePath, IEnumerable<DrugProduct> drugProducts);
        List<TherapeuticClass> ParseTherapeuticFile(string filePath, IEnumerable<DrugProduct> drugProducts);
        List<VeterinarySpecies> ParseVeterinarySpeciesFile(string filePath, IEnumerable<DrugProduct> drugProducts);
    }

}