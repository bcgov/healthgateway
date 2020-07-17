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
namespace HealthGateway.Laboratory.Models
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The user representation of laboratory data.
    /// </summary>
    public class LaboratoryResult
    {
        /// <summary>
        /// Gets or sets the id for the lab result.
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the test type.
        /// </summary>
        [JsonPropertyName("testType")]
        public string TestType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the result is out of range.
        /// </summary>
        [JsonPropertyName("outOfRange")]
        public bool OutOfRange { get; set; }

        /// <summary>
        /// Gets or sets the datetime the lab collection took place.
        /// </summary>
        [JsonPropertyName("collectedDateTime")]
        public DateTime CollectionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the status of the test.
        /// </summary>
        [JsonPropertyName("testStatus")]
        public string? TestStatus { get; set; }

        /// <summary>
        /// Gets or sets the result description.
        /// </summary>
        [JsonPropertyName("resultDescription")]
        public string ResultDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the received datetime.
        /// </summary>
        [JsonPropertyName("receivedDateTime")]
        public DateTime ReceivedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the result datetime.
        /// </summary>
        [JsonPropertyName("resultDateTime")]
        public DateTime ResultDateTime { get; set; }

        /// <summary>
        /// Gets or sets the LOINC code.
        /// </summary>
        [JsonPropertyName("loinc")]
        public string? LOINC { get; set; }

        /// <summary>
        /// Gets or sets the LOINC Name/Description.
        /// </summary>
        [JsonPropertyName("loincName")]
        public string? LOINCName { get; set; }
    }
}
