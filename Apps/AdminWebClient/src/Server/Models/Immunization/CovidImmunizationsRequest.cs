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
namespace HealthGateway.Admin.Models.Immunization
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a request to retrieve immunizationData.
    /// </summary>
    public class CovidImmunizationsRequest
    {
        /// <summary>
        /// Gets or sets the personal health number.
        /// </summary>
        [JsonPropertyName("phn")]
        public string PersonalHealthNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the request should ignore the cached data.
        /// </summary>
        [JsonPropertyName("ignoreCache")]
        public bool IgnoreCache { get; set; } = false;
    }
}
