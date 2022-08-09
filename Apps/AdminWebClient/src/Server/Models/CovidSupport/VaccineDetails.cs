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
namespace HealthGateway.Admin.Models.CovidSupport
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// Represents the retrieved vaccination information.
    /// </summary>
    public class VaccineDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineDetails"/> class.
        /// </summary>
        public VaccineDetails()
        {
            this.Doses = new List<VaccineDose>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineDetails"/> class.
        /// </summary>
        /// <param name="doses">The list of doses.</param>
        /// <param name="vaccineStatusResult">The patient's vaccine status.</param>
        [JsonConstructor]
        public VaccineDetails(IList<VaccineDose> doses, VaccineStatusResult vaccineStatusResult)
        {
            this.Doses = doses;
            this.VaccineStatusResult = vaccineStatusResult;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the requested record has been protected from being accessed.
        /// </summary>
        public bool Blocked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the requested record contains any invalid doses.
        /// </summary>
        public bool ContainsInvalidDoses { get; set; }

        /// <summary>
        /// Gets the retrieved doses. Empty if no valid COVID-19 doses were found.
        /// </summary>
        public IList<VaccineDose> Doses { get; }

        /// <summary>
        /// Gets or sets the Vaccine Status Result.
        /// </summary>
        [JsonPropertyName("vaccineStatus")]
        public VaccineStatusResult? VaccineStatusResult { get; set; }
    }
}
