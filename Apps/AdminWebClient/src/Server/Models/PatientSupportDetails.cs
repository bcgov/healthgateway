// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Admin.Models
{
    using System;

    /// <summary>
    /// Represents details associated with a patient retrieved by a support query.
    /// </summary>
    public class PatientSupportDetails
    {
        /// <summary>
        /// Gets or sets the patient's PHN.
        /// </summary>
        public string PersonalHealthNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the patient's hdid.
        /// </summary>
        public string Hdid { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's created datetime.
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the user's last login datetime.
        /// </summary>
        public DateTime? LastLoginDateTime { get; set; }

        /// <summary>
        /// Gets or sets the physical address for the patient.
        /// </summary>
        public string PhysicalAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the postal address for the patient.
        /// </summary>
        public string PostalAddress { get; set; } = string.Empty;
    }
}
