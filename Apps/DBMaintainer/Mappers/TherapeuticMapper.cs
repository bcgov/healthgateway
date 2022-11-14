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
    public sealed class TherapeuticMapper : ClassMap<TherapeuticClass>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TherapeuticMapper"/> class.
        /// Performs the mapping of the read file to the to the model.
        /// </summary>
        /// <param name="drugProducts">The DrugProduct to relate the object to.</param>
        public TherapeuticMapper(IEnumerable<DrugProduct> drugProducts)
        {
            // DRUG_CODE
            this.Map(m => m.DrugProductId).Convert(converter => drugProducts.First(d => d.DrugCode == converter.Row.GetField(0)).Id);

            // TC_ATC_NUMBER
            this.Map(m => m.AtcNumber).Index(1);

            // TC_ATC
            this.Map(m => m.Atc).Index(2);

            // TC_AHFS_NUMBER
            this.Map(m => m.AhfsNumber).Index(3);

            // TC_AHFS
            this.Map(m => m.Ahfs).Index(4);

            // TC_ATC_F
            this.Map(m => m.AtcFrench).Index(5);

            // TC_AHFS_F
            this.Map(m => m.AhfsFrench).Index(6);
        }
    }
}
