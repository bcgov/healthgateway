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
    using CsvHelper.Configuration;
    using HealthGateway.Database.Models;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Mapping class to which maps the read file to the relavent model object.
    /// </summary>
    public class RouteMapper : ClassMap<Route>
    {
        /// <summary>
        /// Performs the mapping of the read file to the to the model.
        /// </summary>
        /// <param name="drugProducts">The DrugProduct to relate the object to.</param>
        public RouteMapper(IEnumerable<DrugProduct> drugProducts)
        {
            // DRUG_CODE
            Map(m => m.DrugProductId).ConvertUsing(row => drugProducts.Where(d => d.DrugCode == row.GetField(0)).First().Id);
            //ROUTE_OF_ADMINISTRATION_CODE
            Map(m => m.AdministrationCode).Index(1);
            //ROUTE_OF_ADMINISTRATION
            Map(m => m.Administration).Index(2);
            //ROUTE_OF_ADMINISTRATION_F
            Map(m => m.AdministrationFrench).Index(3);
        }
    }
}