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
namespace HealthGateway.Common.Constants
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the vaccination status.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum VaccinationStatus
    {
        /// <summary>
        /// The status code for a fully vaccinated individual.
        /// </summary>
        [EnumMember(Value = "Fully")]
        Fully,

        /// <summary>
        /// The status code for a partially vaccinated individual.
        /// </summary>
        [EnumMember(Value = "Partially")]
        Partially,

        /// <summary>
        /// The status code for an exempt individual.
        /// </summary>
        [EnumMember(Value = "Exempt")]
        Exempt,

        /// <summary>
        /// The status code that represents an unknown state.
        /// </summary>
        [EnumMember(Value = "Unknown")]
        Unknown,
    }
}
