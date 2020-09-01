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
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a row in the Immunization Model.
    /// </summary>
    public class ImmunizationModel
    {
        /// <summary>
        /// Gets or sets the Immunization id.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the immunization Is Self Reported.
        /// </summary>
        [JsonPropertyName("isSelfReported")]
        public bool IsSelfReported { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Type.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Immunized Date and Time.
        /// </summary>
        [JsonPropertyName("immunized")]
        public DateTime ImmunizedAt { get; set; }
    }
}
