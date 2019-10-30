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
    using HealthGateway.Database.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Interface to parse each of the Federal Government drug files.
    /// </summary>
    public interface IDrugProductParser
    {
        /// <summary>
        /// Parses the Drug file.
        /// </summary>
        /// <param name="sourcefolder">The source folder of the extracted files.</param>
        /// <param name="filedownload">The file download to assoicate to the parsed records.</param>
        /// <returns></returns>
        List<DrugProduct> ParseDrugFile(string sourcefolder, FileDownload filedownload);

        /// <summary>
        /// Parses the Active Ingredient file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="drugProducts">The Drug products to link.</param>
        /// <returns></returns>
        List<ActiveIngredient> ParseActiveIngredientFile(string filePath, IEnumerable<DrugProduct> drugProducts);

        /// <summary>
        /// Parses the company file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="drugProducts">The Drug products to link.</param>
        /// <returns></returns>
        List<Company> ParseCompanyFile(string filePath, IEnumerable<DrugProduct> drugProducts);

        /// <summary>
        /// Parses the status file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="drugProducts">The Drug products to link.</param>
        /// <returns></returns>
        List<Status> ParseStatusFile(string filePath, IEnumerable<DrugProduct> drugProducts);

        /// <summary>
        /// Parses the form file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="drugProducts">The Drug products to link.</param>
        /// <returns></returns>
        List<Form> ParseFormFile(string filePath, IEnumerable<DrugProduct> drugProducts);

        /// <summary>
        /// Parses the Packaging file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="drugProducts">The Drug products to link.</param>
        /// <returns></returns>
        List<Packaging> ParsePackagingFile(string filePath, IEnumerable<DrugProduct> drugProducts);

        /// <summary>
        /// Parses the pharmaceutical standard file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="drugProducts">The Drug products to link.</param>
        /// <returns></returns>
        List<PharmaceuticalStd> ParsePharmaceuticalStdFile(string filePath, IEnumerable<DrugProduct> drugProducts);

        /// <summary>
        /// Parses the route file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="drugProducts">The Drug products to link.</param>
        /// <returns></returns>
        List<Route> ParseRouteFile(string filePath, IEnumerable<DrugProduct> drugProducts);

        /// <summary>
        /// Parses the schedule file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="drugProducts">The Drug products to link.</param>
        /// <returns></returns>
        List<Schedule> ParseScheduleFile(string filePath, IEnumerable<DrugProduct> drugProducts);

        /// <summary>
        /// Parses the Therapeutic class file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="drugProducts">The Drug products to link.</param>
        /// <returns></returns>
        List<TherapeuticClass> ParseTherapeuticFile(string filePath, IEnumerable<DrugProduct> drugProducts);

        /// <summary>
        /// Parses the verterinary species file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="drugProducts">The Drug products to link.</param>
        /// <returns></returns>
        List<VeterinarySpecies> ParseVeterinarySpeciesFile(string filePath, IEnumerable<DrugProduct> drugProducts);
    }

}