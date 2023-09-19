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
namespace HealthGateway.Admin.Common.Models
{
    using System;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Represents details associated with a patient retrieved by a support query.
    /// </summary>
    public class PatientSupportResult
    {
        /// <summary>
        /// Gets or sets the patient's status.
        /// </summary>
        public PatientStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a warning message associated with the patient.
        /// </summary>
        public string WarningMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the patient's HDID.
        /// </summary>
        public string Hdid { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the patient's PHN.
        /// </summary>
        public string PersonalHealthNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the patient's common name.
        /// </summary>
        public Name? CommonName { get; set; }

        /// <summary>
        /// Gets or sets the patient's legal name.
        /// </summary>
        public Name? LegalName { get; set; }

        /// <summary>
        /// Gets the patient's preferred name.
        /// </summary>
        public Name? PreferredName => this.CommonName ?? this.LegalName;

        /// <summary>
        /// Gets or sets the patient's date of birth.
        /// </summary>
        public DateOnly? Birthdate { get; set; }

        /// <summary>
        /// Gets or sets the physical address for the patient.
        /// </summary>
        public Address? PhysicalAddress { get; set; }

        /// <summary>
        /// Gets or sets the postal address for the patient.
        /// </summary>
        public Address? PostalAddress { get; set; }

        /// <summary>
        /// Gets or sets the user's created datetime.
        /// </summary>
        public DateTime? ProfileCreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the user's last login datetime.
        /// </summary>
        public DateTime? ProfileLastLoginDateTime { get; set; }
    }
}
