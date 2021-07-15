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

namespace HealthGateway.Common.Models
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The patient data model.
    /// </summary>
    public class PatientModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatientModel"/> class.
        /// </summary>
        public PatientModel()
        {
            this.HdId = string.Empty;
            this.PersonalHealthNumber = string.Empty;
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.EmailAddress = string.Empty;
            this.Gender = string.Empty;
        }

        /// <summary>
        /// Gets or sets the health directed identifier.
        /// </summary>
        [JsonPropertyName("hdid")]
        public string HdId { get; set; }

        /// <summary>
        /// Gets or sets the patient PHN.
        /// </summary>
        [JsonPropertyName("personalhealthnumber")]
        public string PersonalHealthNumber { get; set; }

        /// <summary>
        /// Gets or sets the patient first name.
        /// </summary>
        [JsonPropertyName("firstname")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the patient last name.
        /// </summary>
        [JsonPropertyName("lastname")]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the patients date of birth.
        /// </summary>
        [JsonPropertyName("birthdate")]
        public DateTime Birthdate { get; set; }

        /// <summary>
        /// Gets or sets the patients email.
        /// </summary>
        [JsonPropertyName("email")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the patients gender.
        /// </summary>
        [JsonPropertyName("gender")]
        public string Gender { get; set; }
    }
}
