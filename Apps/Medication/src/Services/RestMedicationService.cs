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
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Medication.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class RestMedicationService : IMedicationService
    {
        private readonly ILogger logger;
        private readonly IDrugLookupDelegate drugLookupDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedicationService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="drugLookupDelegate">The injected drug lookup delegate.</param>
        public RestMedicationService(
            ILogger<RestMedicationService> logger,
            IDrugLookupDelegate drugLookupDelegate)
        {
            this.logger = logger;
            this.drugLookupDelegate = drugLookupDelegate;
        }

        /// <inheritdoc/>
        public IDictionary<string, MedicationInformation> GetMedications(IList<string> medicationDinList)
        {
            this.logger.LogTrace("Getting list of medications...");
            IDictionary<string, MedicationInformation> result = new Dictionary<string, MedicationInformation>();

            // Retrieve drug information from the Federal soruce
            IList<DrugProduct> drugProducts = this.drugLookupDelegate.GetDrugProductsByDin(medicationDinList);
            foreach (DrugProduct drugProduct in drugProducts)
            {
                FederalDrugSource federalData = new()
                {
                    UpdateDateTime = drugProduct.UpdatedDateTime,
                    DrugProduct = drugProduct,
                };
                result[drugProduct.DrugIdentificationNumber] = new MedicationInformation
                {
                    DIN = drugProduct.DrugIdentificationNumber,
                    FederalData = federalData,
                };
            }

            // Retrieve drug information from the Provincial source and append it to the result if previously added.
            IList<PharmaCareDrug> pharmaCareDrugs = this.drugLookupDelegate.GetPharmaCareDrugsByDin(medicationDinList);
            foreach (PharmaCareDrug pharmaCareDrug in pharmaCareDrugs)
            {
                ProvincialDrugSource provincialData = new()
                {
                    UpdateDateTime = pharmaCareDrug.UpdatedDateTime,
                    PharmaCareDrug = pharmaCareDrug,
                };

                if (result.ContainsKey(pharmaCareDrug.DinPin))
                {
                    result[pharmaCareDrug.DinPin].ProvincialData = provincialData;
                }
                else
                {
                    result[pharmaCareDrug.DinPin] = new MedicationInformation
                    {
                        DIN = pharmaCareDrug.DinPin,
                        ProvincialData = provincialData,
                    };
                }
            }

            this.logger.LogDebug("Finished getting list of medications.");
            return result;
        }
    }
}
