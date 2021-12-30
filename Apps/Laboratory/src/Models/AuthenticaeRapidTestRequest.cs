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
    /// Object that defines the request for submitting a rapid test.
    /// </summary>
    public class AuthenticaeRapidTestRequest
    {
        /// <summary>
        /// Gets or sets the id for the lab result.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the PHN the report is for.
        /// </summary>
        [JsonPropertyName("phn")]
        public string PHN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the serial number for rapid test.
        /// </summary>
        [JsonPropertyName("labSerialNumber")]
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether result is positive (true) or negative (false).
        /// </summary>
        [JsonPropertyName("positive")]
        public bool Result { get; set; } = false;

        /// <summary>
        /// Gets or sets the date of rapid test.
        /// </summary>
        [JsonPropertyName("dateTestTaken")]
        public DateTime DateTestTaken { get; set; }
    }
}
