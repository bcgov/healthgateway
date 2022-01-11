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
namespace HealthGateway.Common.Models.PHSA
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The representation of a COVID-19 test result.
    /// </summary>
    public class CovidTestResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CovidTestResult"/> class.
        /// </summary>
        public CovidTestResult()
        {
            this.ResultDescription = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CovidTestResult"/> class.
        /// </summary>
        /// <param name="resultDescription">The list of result descriptions.</param>
        [JsonConstructor]
        public CovidTestResult(IList<string> resultDescription)
        {
            this.ResultDescription = resultDescription;
        }

        /// <summary>
        /// Gets or sets the first name and last initial of the patient.
        /// </summary>
        [JsonPropertyName("patientDisplayName")]
        public string PatientDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the reporting lab.
        /// </summary>
        [JsonPropertyName("reportingLab")]
        public string Lab { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the report ID.
        /// </summary>
        [JsonPropertyName("reportId")]
        public string ReportId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date time when the lab collection took place.
        /// </summary>
        [JsonPropertyName("collectionDateTime")]
        public DateTime? CollectionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the result date time.
        /// </summary>
        [JsonPropertyName("resultDateTime")]
        public DateTime? ResultDateTime { get; set; }

        /// <summary>
        /// Gets or sets the test name.
        /// </summary>
        [JsonPropertyName("covid19TestName")]
        public string TestName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the test type.
        /// </summary>
        [JsonPropertyName("covid19TestType")]
        public string TestType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the test from the lab.
        /// </summary>
        [JsonPropertyName("labStatus")]
        public string TestStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the outcome of the test from the lab.
        /// </summary>
        [JsonPropertyName("labOutcome")]
        public string TestOutcome { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the result title.
        /// </summary>
        [JsonPropertyName("resultTitle")]
        public string ResultTitle { get; set; } = string.Empty;

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
        /// Gets or sets the state of the response.
        /// </summary>
        [JsonPropertyName("statusIndicator")]
        public string StatusIndicator { get; set; } = string.Empty;
    }
}
