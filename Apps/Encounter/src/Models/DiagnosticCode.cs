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
namespace HealthGateway.Encounter.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a row in the Diagnostic Code.
    /// </summary>
    public class DiagnosticCode
    {
        /// <summary>
        /// Gets or sets the Diagnostic Code 1.
        /// </summary>
        [JsonPropertyName("diagCode1")]
        public string DiagCode1 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Diagnostic Code 2.
        /// </summary>
        [JsonPropertyName("diagCode2")]
        public string DiagCode2 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Diagnostic Code 3.
        /// </summary>
        [JsonPropertyName("diagCode3")]
        public string DiagCode3 { get; set; } = string.Empty;
    }
}
