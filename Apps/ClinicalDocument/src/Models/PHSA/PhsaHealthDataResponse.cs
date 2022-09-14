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
namespace HealthGateway.ClinicalDocument.Models.PHSA
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the health data response for clinical document records.
    /// </summary>
    public class PhsaHealthDataResponse
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        [JsonPropertyName("data")]
        public IEnumerable<PhsaClinicalDocumentRecord> Data { get; set; } = new List<PhsaClinicalDocumentRecord>();
    }
}
