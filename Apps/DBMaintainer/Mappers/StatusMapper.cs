//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.DBMaintainer.Mappers
{
    using System.Collections.Generic;
    using System.Linq;
    using CsvHelper.Configuration;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Mapping class to which maps the read file to the relavent model object.
    /// </summary>
    public sealed class StatusMapper : ClassMap<Status>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusMapper"/> class.
        /// Performs the mapping of the read file to the to the model.
        /// </summary>
        /// <param name="drugProducts">The DrugProduct to relate the object to.</param>
        public StatusMapper(IEnumerable<DrugProduct> drugProducts)
        {
            // DRUG_CODE
            this.Map(m => m.DrugProductId).Convert(converter => drugProducts.First(d => d.DrugCode == converter.Row.GetField(0)).Id);

            // CURRENT_STATUS_FLAG
            this.Map(m => m.CurrentStatusFlag).Index(1);

            // STATUS
            this.Map(m => m.StatusDesc).Index(2);

            // HISTORY_DATE
            this.Map(m => m.HistoryDate).Index(3);

            // STATUS_F
            this.Map(m => m.StatusDescFrench).Index(4);

            // LOT_NUMBER
            this.Map(m => m.LotNumber).Index(5);

            // EXPIRATION_DATE
            this.Map(m => m.ExpirationDate).Index(6);
        }
    }
}
