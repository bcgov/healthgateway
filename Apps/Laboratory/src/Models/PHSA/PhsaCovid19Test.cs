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
namespace HealthGateway.Laboratory.Models.PHSA
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The user representation of COVID-19 data.
    /// </summary>
    public class PhsaCovid19Test
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhsaCovid19Test"/> class.
        /// </summary>
        public PhsaCovid19Test()
        {
            this.ResultDescription = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhsaCovid19Test"/> class.
        /// </summary>
        /// <param name="resultDescription">The list of result descriptions.</param>
        [JsonConstructor]
        public PhsaCovid19Test(IList<string> resultDescription)
        {
            this.ResultDescription = resultDescription;
        }

        /// <summary>
        /// Gets or sets the id for the COVID-19 result.
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
        public DateTime CollectedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the status of the test.
        /// </summary>
        [JsonPropertyName("testStatus")]
        public string? TestStatus { get; set; }

        /// <summary>
        /// Gets or sets the lab result outcome.
        /// </summary>
        [JsonPropertyName("labResultOutcome")]
        public string LabResultOutcome { get; set; } = string.Empty;

        /// <summary>
        /// Gets the result description.
        /// </summary>
        [JsonPropertyName("resultDescription")]
        public IList<string> ResultDescription { get; }

        /// <summary>
        /// Gets or sets the result link.
        /// </summary>
        [JsonPropertyName("resultLink")]
        public string ResultLink { get; set; } = string.Empty;

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
