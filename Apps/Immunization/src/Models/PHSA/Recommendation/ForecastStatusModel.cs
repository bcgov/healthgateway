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
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The PHSA Forecast Status data model.
    /// </summary>
    public class ForecastStatusModel
    {
        /// <summary>
        /// Gets the Forecast Status codes.
        /// </summary>
        [JsonPropertyName("forcastCodes")]
        public IList<SystemCode> ForcastCodes { get; } = new List<SystemCode>();

        /// <summary>
        /// Gets or sets the Date Criterion value.
        /// </summary>
        [JsonPropertyName("forecastStatusText")]
        public string ForecastStatusText { get; set; } = string.Empty;
    }
}
