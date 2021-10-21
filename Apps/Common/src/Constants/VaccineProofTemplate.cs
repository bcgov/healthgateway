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
    /// Represents the templates that can be requested.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum VaccineProofTemplate
    {
        /// <summary>
        /// Indicates that the BC Provincial template should be used.
        /// </summary>
        [EnumMember(Value = "BCProvincial")]
        Provincial,

        /// <summary>
        /// Indicates that the Federal template should be used.
        /// </summary>
        [EnumMember(Value = "Federal")]
        Federal,

        /// <summary>
        /// Indicates that the Federal and BC Provincial template should be used.
        /// </summary>
        [EnumMember(Value = "Combined")]
        Combined,

        /// <summary>
        /// Indicates that the Cover Page plus Federal and BC Provincial template should be used.
        /// </summary>
        [EnumMember(Value = "CombinedCover")]
        CombinedCover,
    }
}
