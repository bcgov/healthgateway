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
namespace HealthGateway.Medication.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Medication.Models;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class RestMedicationService : IMedicationService
    {
        private readonly IDrugLookupDelegate drugLookupDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedicationService"/> class.
        /// </summary>
        /// <param name="drugLookupDelegate">The injected drug lookup delegate.</param>
        public RestMedicationService(
            IDrugLookupDelegate drugLookupDelegate)
        {
            this.drugLookupDelegate = drugLookupDelegate;
        }

        /// <inheritdoc/>
        public IDictionary<string, MedicationInformation> GetMedications(IList<string> medicationDinList)
        {
            IList<DrugProduct> drugProducts = this.drugLookupDelegate.GetDrugProductsByDin(medicationDinList);

            Dictionary<string, MedicationInformation> drugs = drugProducts.ToDictionary(
                m => m.DrugIdentificationNumber,
                m => new MedicationInformation
                {
                    Din = m.DrugIdentificationNumber,
                    FederalData = new FederalDrugSource
                    {
                        DrugProduct = m,
                        UpdateDateTime = m.UpdatedDateTime,
                    },
                });

            IList<PharmaCareDrug> pharmaCareDrugs = this.drugLookupDelegate.GetPharmaCareDrugsByDin(medicationDinList);

            foreach (PharmaCareDrug pharmaDrug in pharmaCareDrugs)
            {
                if (!drugs.TryGetValue(pharmaDrug.DinPin, out MedicationInformation? drug))
                {
                    // add pharma drug
                    drugs.Add(
                        pharmaDrug.DinPin,
                        new MedicationInformation
                        {
                            Din = pharmaDrug.DinPin,
                            ProvincialData = new ProvincialDrugSource
                            {
                                PharmaCareDrug = pharmaDrug,
                                UpdateDateTime = pharmaDrug.UpdatedDateTime,
                            },
                        });
                }
                else
                {
                    // attach to existing drug
                    drug.ProvincialData = new ProvincialDrugSource
                    {
                        PharmaCareDrug = pharmaDrug,
                        UpdateDateTime = pharmaDrug.UpdatedDateTime,
                    };
                }
            }

            return drugs;
        }
    }
}
