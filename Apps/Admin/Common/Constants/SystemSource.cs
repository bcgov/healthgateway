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
    /// Represents the system source to be queried with.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SystemSource
    {
        /// <summary>
        /// Specifies that the Diagnostic Imaging is the system source.
        /// </summary>
        DiagnosticImaging,

        /// <summary>
        /// Specifies that the Laboratory is the system source.
        /// </summary>
        Laboratory,
    }
}
