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
    public sealed class FormMapper : ClassMap<Form>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormMapper"/> class.
        /// Performs the mapping of the read file to the to the model.
        /// </summary>
        /// <param name="drugProducts">The DrugProduct to relate the object to.</param>
        public FormMapper(IEnumerable<DrugProduct> drugProducts)
        {
            // DRUG_CODE
            this.Map(m => m.DrugProductId).Convert(converter => drugProducts.First(d => d.DrugCode == converter.Row.GetField(0)).Id);

            // PHARM_FORM_CODE
            this.Map(m => m.PharmaceuticalFormCode).Index(1);

            // PHARMACEUTICAL_FORM
            this.Map(m => m.PharmaceuticalForm).Index(2);

            // PHARMACEUTICAL_FORM_F
            this.Map(m => m.PharmaceuticalFormFrench).Index(3);
        }
    }
}
