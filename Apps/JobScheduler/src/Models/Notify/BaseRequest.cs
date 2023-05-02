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
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the common properties for a Notify Request.
    /// </summary>
    public class BaseRequest
    {
        /// <summary>
        /// Gets the template id to use.
        /// </summary>
        [JsonPropertyName("template_id")]
        public required Guid TemplateId { get; init; }

        /// <summary>
        /// Gets the personalization data.
        /// </summary>
        [JsonPropertyName("personalisation")]
        public Dictionary<string, string>? Personalization { get; init; }

        /// <summary>
        /// Gets or sets the reference.
        /// </summary>
        [JsonPropertyName("reference")]
        public string? Reference { get; set; }
    }
}
