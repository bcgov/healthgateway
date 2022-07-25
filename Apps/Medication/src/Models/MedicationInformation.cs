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
namespace HealthGateway.Medication.Models
{
    /// <summary>
    /// The medications data model.
    /// </summary>
    public class MedicationInformation
    {
        /// <summary>
        /// Gets or sets the Drug Identification Number for the prescribed medication.
        /// </summary>
        public string? DIN { get; set; }

        /// <summary>
        /// Gets or sets the Federal Drug Data Source.
        /// </summary>
        public FederalDrugSource? FederalData { get; set; }

        /// <summary>
        /// Gets or sets the Provincial Drug Data Source.
        /// </summary>
        public ProvincialDrugSource? ProvincialData { get; set; }
    }
}
