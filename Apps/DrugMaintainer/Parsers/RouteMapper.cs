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
    using HealthGateway.Common.Database.Models;
    using System.Collections.Generic;
    using System.Linq;

    public class RouteMapper : ClassMap<Route>
    {
        public RouteMapper(IEnumerable<DrugProduct> drugProducts)
        {
            // DRUG_CODE
            Map(m => m.Drug).ConvertUsing(row => drugProducts.Where(d => d.DrugCode == row.GetField(0)).First());
            //ROUTE_OF_ADMINISTRATION_CODE
            Map(m => m.AdministrationCode).Index(1);
            //ROUTE_OF_ADMINISTRATION
            Map(m => m.Administration).Index(2);
            //ROUTE_OF_ADMINISTRATION_F
            Map(m => m.AdministrationFrench).Index(3);
        }
    }
}