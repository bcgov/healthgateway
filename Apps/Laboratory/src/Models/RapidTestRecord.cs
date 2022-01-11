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
    /// The representation of a rapid test record for authenticated access.
    /// </summary>
    public class RapidTestRecord
    {
        /// <summary>
        /// Gets or sets the rapid test result of the patient.
        /// </summary>
        [JsonPropertyName("testOutcome")]
        public string TestResult { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the serial number of the rapid test kit.
        /// </summary>
        [JsonPropertyName("labSerialNumber")]
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date when the rapid test is taken.
        /// </summary>
        [JsonPropertyName("dateTestTaken")]
        public DateTime TestTakenDate { get; set; }
    }
}
