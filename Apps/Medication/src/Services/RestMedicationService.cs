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
        public RestMedicationService(IDrugLookupDelegate drugLookupDelegate)
        {
            this.drugLookupDelegate = drugLookupDelegate;
        }

        /// <inheritdoc/>
        public Dictionary<string, MedicationResult> GetMedications(List<string> medicationDinList)
        {
            Dictionary<string, MedicationResult> result = new Dictionary<string, MedicationResult>();

            // Retrieve drug information from the Federal soruce
            List<DrugProduct> drugProducts = this.drugLookupDelegate.GetDrugProductsByDIN(medicationDinList);
            foreach (DrugProduct drugProduct in drugProducts)
            {
                FederalDrugSource federalData = new FederalDrugSource()
                {
                    UpdateDateTime = drugProduct.UpdatedDateTime,
                    DrugProduct = drugProduct,
                };
                result[drugProduct.DrugIdentificationNumber] = new MedicationResult() { DIN = drugProduct.DrugIdentificationNumber, FederalData = federalData };
            }

            // Retrieve drug information from the Provincial source and append it to the result if previously added.
            List<PharmaCareDrug> pharmaCareDrugs = this.drugLookupDelegate.GetPharmaCareDrugsByDIN(medicationDinList);
            foreach (PharmaCareDrug pharmaCareDrug in pharmaCareDrugs)
            {
                ProvincialDrugSource provincialData = new ProvincialDrugSource()
                {
                    UpdateDateTime = pharmaCareDrug.UpdatedDateTime,
                    PharmaCareDrug = pharmaCareDrug,
                };

                if (result.ContainsKey(pharmaCareDrug.DINPIN))
                {
                    result[pharmaCareDrug.DINPIN].ProvincialData = provincialData;
                }
                else
                {
                    result[pharmaCareDrug.DINPIN] = new MedicationResult() { DIN = pharmaCareDrug.DINPIN, ProvincialData = provincialData };
                }
            }

            return result;
        }
    }
}