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
namespace HealthGateway.Immunization.Models
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The PHSA immunization request.
    /// </summary>
    public class ImmunizationRequest
    {
        /// <summary>
        /// Gets or sets the patient PHN.
        /// </summary>
        [JsonPropertyName("phn")]
        public string? PersonalHealthNumber { get; set; }

        /// <summary>
        /// Gets or sets the patient date of birth.
        /// </summary>
        [JsonIgnore]        
        public DateTime? DateOfBirth { get; set; }


        /// <summary>
        /// Gets or sets the patient date of birth in YYYYMMDD format.
        /// </summary>
        [JsonPropertyName("dob")]
        public string? FormattedDateOfBirth {
        get { return this.DateOfBirth?.ToString("yyyyMMdd"); }
        set { this.DateOfBirth = DateTime.Parse(value ?? ""); }
    }
    }
}
