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
namespace HealthGateway.DBMaintainer.Models
{
    /// <summary>
    /// Represents a row in the Pharmacy Assessment file
    /// https://github.com/bcgov/pharmacy-assessment/blob/main/Pharmacy%20Assessment%20PIN.csv.
    /// </summary>
    public class PharmacyAssessment
    {
        /// <summary>
        /// Gets or sets the pin.
        /// </summary>
        public string Pin { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the pharmacy assessment title.
        /// </summary>
        public string? PharmacyAssessmentTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether prescription is provided.
        /// </summary>
        public bool? PrescriptionProvided { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether redirected to health care provider is set.
        /// </summary>
        public bool? RedirectedToHealthCareProvider { get; set; }
    }
}
