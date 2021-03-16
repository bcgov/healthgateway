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
    using System;

    /// <summary>
    /// The medications request data model.
    /// </summary>
    public class MedicationRequest
    {
        /// <summary>
        /// Gets or sets the reference number.
        /// </summary>
        public string ReferenceNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the drug name.
        /// </summary>
        public string? DrugName { get; set; }

        /// <summary>
        /// Gets or sets the request status.
        /// </summary>
        public string? RequestStatus { get; set; }

        /// <summary>
        /// Gets or sets the prescriber's firstname.
        /// </summary>
        public string? PrescriberFirstName { get; set; }

        /// <summary>
        /// Gets or sets the prescriber's lastname.
        /// </summary>
        public string? PrescriberLastName { get; set; }

        /// <summary>
        /// Gets or sets the requested date.
        /// </summary>
        public DateTime RequestedDate { get; set; }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expiry date.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }
    }
}
