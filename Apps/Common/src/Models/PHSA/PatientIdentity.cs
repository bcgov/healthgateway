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
    public class PatientIdentity
    {
        /// <summary>
        /// Gets or sets PHSA patient identifier.
        /// </summary>
        [JsonPropertyName("pid")]
        public Guid Pid { get; set; }

        /// <summary>
        /// Gets or sets the personal health number.
        /// </summary>
        [JsonPropertyName("phn")]
        public string? Phn { get; set; }

        /// <summary>
        /// Gets or sets the hdid.
        /// </summary>
        [JsonPropertyName("hdid")]
        public string? HdId { get; set; }

        /// <summary>
        /// Gets or sets the given name.
        /// </summary>
        [JsonPropertyName("firstname")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the surname.
        /// </summary>
        [JsonPropertyName("lastname")]
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the birthdate.
        /// </summary>
        [JsonPropertyName("birthdate")]
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

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
    }
}
