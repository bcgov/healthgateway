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
namespace HealthGateway.Admin.Models.Support
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Immunization;

    /// <summary>
    /// Represents the retrieved immunization information.
    /// </summary>
    public class CovidInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CovidInformation"/> class.
        /// </summary>
        public CovidInformation()
        {
            this.Immunizations = new List<ImmunizationEvent>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CovidInformation"/> class.
        /// </summary>
        /// <param name="patient">The patient information.</param>
        /// <param name="immunizations">The list of immunizations.</param>
        [JsonConstructor]
        public CovidInformation(PatientModel patient, IList<ImmunizationEvent> immunizations)
        {
            this.Patient = patient;
            this.Immunizations = immunizations;
        }

        /// <summary>
        /// Gets or sets the retrieved patient information. Null if no person retrieved.
        /// </summary>
        public PatientModel? Patient { get; set; }

        /// <summary>
        /// Gets the retrieved immunizations. Empty if no covid immunizations where found.
        /// </summary>
        public IList<ImmunizationEvent> Immunizations { get; }
    }
}
