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
namespace HealthGateway.WebClient.Constants
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Notification Targets for PHSA Notificaiton Settings.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum NotificationTarget
    {
        /// <summary>
        /// Specifies the target is for Covid19.
        /// </summary>
        [EnumMember(Value = "COVID19")]
        Covid19,

        /// <summary>
        /// Specifies that the target is for Labs.
        /// </summary>
        [EnumMember(Value = "LABS")]
        Labs,

        /// <summary>
        /// Specifies that the target is for Imaging.
        /// </summary>
        [EnumMember(Value = "IMAGING")]
        Imaging,
    }
}
