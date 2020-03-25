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
namespace HealthGateway.Medication.Constants
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Protective Word Maintenance Operation to be performed.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum ProtectiveWordOperator
    {
        /// <summary>
        /// Perform the Get operation.
        /// </summary>
        [EnumMember(Value = "get")]
        Get,

        /// <summary>
        /// Perform the Set operation.
        /// </summary>
        [EnumMember(Value = "set")]
        Set,

        /// <summary>
        /// Perform the delete operation.
        /// </summary>
        [EnumMember(Value = "delete")]
        Delete,
    }
}
