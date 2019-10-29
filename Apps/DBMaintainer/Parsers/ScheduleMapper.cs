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
    public class ScheduleMapper : ClassMap<Schedule>
    {
        /// <summary>
        /// Performs the mapping of the read file to the to the model.
        /// </summary>
        /// <param name="drugProducts">The DrugProduct to relate the object to.</param>
        public ScheduleMapper(IEnumerable<DrugProduct> drugProducts)
        {
            // DRUG_CODE
            Map(m => m.DrugProduct).ConvertUsing(row => drugProducts.Where(d => d.DrugCode == row.GetField(0)).First());
            // SCHEDULE
            Map(m => m.ScheduleDesc).Index(1);
            // SCHEDULE_FFootnote
            Map(m => m.ScheduleDescFrench).Index(2);
        }
    }
}