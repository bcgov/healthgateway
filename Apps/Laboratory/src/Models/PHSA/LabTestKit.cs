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
    using System.Text.Json.Serialization;

    /// <summary>
    /// Model object representing an authenticated lab test kit.
    /// </summary>
    public class LabTestKit
    {
        /// <summary>
        /// Gets or sets when the test was used.
        /// </summary>
        [JsonRequired]
        [JsonPropertyName("testTakenMinutesAgo")]
        public int TestTakenMinutesAgo { get; set; }

        /// <summary>
        /// Gets or sets the test kit id.
        /// </summary>
        [JsonPropertyName("testKitCid")]
        public string? TestKitId { get; set; }

        /// <summary>
        /// Gets or sets first portion of the short code.
        /// </summary>
        [JsonPropertyName("shortCodeFirst")]
        public string? ShortCodeFirst { get; set; }

        /// <summary>
        /// Gets or sets the second portion of the short code.
        /// </summary>
        [JsonPropertyName("shortCodeSecond")]
        public string? ShortCodeSecond { get; set; }
    }
}
