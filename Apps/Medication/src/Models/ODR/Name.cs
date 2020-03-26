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
namespace HealthGateway.Medication.Models.ODR
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a persons name.
    /// </summary>
    public class Name
    {
        /// <summary>
        /// Gets or sets the Given name.
        /// </summary>
        [JsonPropertyName("firstName")]
        public string GivenName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or Sets the middle initial.
        /// </summary>
        [JsonPropertyName("middleInit")]
        public string MiddleInitial { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Surname.
        /// </summary>
        [JsonPropertyName("lastName")]
        public string Surname { get; set; } = string.Empty;
    }
}
