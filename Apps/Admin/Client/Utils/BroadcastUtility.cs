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
namespace HealthGateway.Admin.Client.Utils
{
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Utilities for interacting with system broadcasts.
    /// </summary>
    public static class BroadcastUtility
    {
        /// <summary>
        /// Returns the formatted representation of a broadcast's action type.
        /// </summary>
        /// <param name="actionType">The broadcast's action type.</param>
        /// <returns>A string containing the formatted representation of a broadcast's action type.</returns>
        public static string FormatActionType(BroadcastActionType actionType)
        {
            return actionType switch
            {
                BroadcastActionType.None => "None",
                BroadcastActionType.InternalLink => "Internal Link",
                BroadcastActionType.ExternalLink => "External Link",
                _ => actionType.ToString(),
            };
        }

        /// <summary>
        /// Returns the formatted representation of a broadcast's enabled status.
        /// </summary>
        /// <param name="enabled">The broadcast's enabled status.</param>
        /// <returns>A string containing the formatted representation of a broadcast's enabled status.</returns>
        public static string FormatEnabled(bool enabled)
        {
            return enabled ? "Publish" : "Draft";
        }
    }
}
