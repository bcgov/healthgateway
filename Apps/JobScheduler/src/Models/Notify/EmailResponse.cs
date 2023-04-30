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
namespace HealthGateway.JobScheduler.Models.Notify
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents an Email Response object for the GC Notify service.
    /// </summary>
    public class EmailResponse
    {
        /// <summary>
        /// Gets or sets the email id.
        /// </summary>
        [JsonPropertyName("id")]
        public required Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the email reference.
        /// </summary>
        [JsonPropertyName("reference")]
        public string? Reference { get; set; }

        /// <summary>
        /// Gets or sets the email content.
        /// </summary>
        [JsonPropertyName("content")]
        public EmailContent? Content { get; set; }

        /// <summary>
        /// Gets or sets the email uri.
        /// </summary>
        [JsonPropertyName("uri")]
        public Uri? Uri { get; set; }

        /// <summary>
        /// Gets or sets the template.
        /// </summary>
        [JsonPropertyName("template")]
        public GcTemplate? Template { get; set; }
    }
}
