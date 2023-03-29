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
namespace HealthGateway.Patient.Models
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Donor registration status.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum OrganDonorRegistrationStatus
    {
        /// <summary>
        /// Registered patient.
        /// </summary>
        Registered,

        /// <summary>
        /// Not registered patient.
        /// </summary>
        [EnumMember(Value = "Not Registered")]
        NotRegistered,

        /// <summary>
        /// Error in registration.
        /// </summary>
        Error,

        /// <summary>
        /// Registration is pending.
        /// </summary>
        Pending,
    }
}
