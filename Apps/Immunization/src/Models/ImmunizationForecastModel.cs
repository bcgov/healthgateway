//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Immunization.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents an immunization forecast returned by the V2 API.
    /// </summary>
    public class ImmunizationForecastModel
    {
        /// <summary>
        /// Gets or sets the forecast status.
        /// </summary>
        [JsonPropertyName("forecastStatus")]
        public string? ForecastStatus { get; set; }

        /// <summary>
        /// Gets or sets the vaccine code.
        /// </summary>
        [JsonPropertyName("vaccineCode")]
        public string? VaccineCode { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the eligible date.
        /// </summary>
        [JsonPropertyName("eligibleDate")]
        public string? EligibleDate { get; set; }

        /// <summary>
        /// Gets or sets the due date.
        /// </summary>
        [JsonPropertyName("dueDate")]
        public string? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the forecast create date.
        /// </summary>
        [JsonPropertyName("forecastCreateDate")]
        public string? ForecastCreateDate { get; set; }
    }
}
