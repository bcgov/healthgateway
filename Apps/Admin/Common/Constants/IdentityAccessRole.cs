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
namespace HealthGateway.Admin.Common.Constants
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// An enum representing identity access realm role names.
    /// </summary>
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum IdentityAccessRole
    {
        /// <summary>
        /// Unknown role.
        /// </summary>
        Unknown,

        /// <summary>
        /// The role associated with general access to the admin website.
        /// </summary>
        AdminUser,

        /// <summary>
        /// The role associated with access to the feedback module on the admin website.
        /// </summary>
        AdminReviewer,

        /// <summary>
        /// The role associated with service desk access to the admin website.
        /// </summary>
        SupportUser,
    }
}
