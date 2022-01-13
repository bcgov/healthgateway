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
namespace HealthGateway.Admin.Server.Models.CovidSupport
{
    using HealthGateway.Common.Models;

    /// <summary>
    /// Represents the retrieved COVID-19 information for a patient.
    /// </summary>
    public class CovidInformation
    {
        /// <summary>
        /// Gets or sets the retrieved patient information.
        /// </summary>
        public PatientModel Patient { get; set; } = new PatientModel();

        /// <summary>
        /// Gets or sets the Vaccine Details.
        /// </summary>
        public VaccineDetails? VaccineDetails { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the requested record has been protected from being accessed.
        /// </summary>
        public bool Blocked { get; set; }
    }
}
