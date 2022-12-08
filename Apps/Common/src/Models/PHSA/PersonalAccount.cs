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
namespace HealthGateway.Common.Models.PHSA
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Model representing a PHSA Personal Account.
    /// </summary>
    public class PersonalAccount
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this account is active.
        /// </summary>
        [JsonPropertyName("active")]
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the created datetime in UTC.
        /// </summary>
        [JsonPropertyName("creationTimeStampUtc")]
        public DateTime CreationTimeStampUtc { get; set; }

        /// <summary>
        /// Gets or sets the modified datetime in UTC.
        /// </summary>
        [JsonPropertyName("modifyTimeStampUtc")]
        public DateTime ModifyTimeStampUtc { get; set; }

        /// <summary>
        /// Gets or sets the Patient Identity.
        /// </summary>
        [JsonPropertyName("patientIdentity")]
        public PatientIdentity PatientIdentity { get; set; } = null!;
    }
}
