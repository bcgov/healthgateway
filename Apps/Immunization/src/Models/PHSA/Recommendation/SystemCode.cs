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
namespace HealthGateway.Immunization.Models.PHSA.Recommendation
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The PHSA System Code data model.
    /// </summary>
    public class SystemCode
    {
        /// <summary>
        /// Gets or sets the Code text.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Common Type.
        /// </summary>
        [JsonPropertyName("commonType")]
        public string CommonType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Display.
        /// </summary>
        [JsonPropertyName("display")]
        public string Display { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        [JsonPropertyName("system")]
        public string System { get; set; } = string.Empty;
    }
}
