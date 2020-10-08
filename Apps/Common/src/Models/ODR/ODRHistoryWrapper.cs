﻿// -------------------------------------------------------------------------
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
namespace HealthGateway.Common.Models.ODR
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The ODR History Request/Response model.
    /// </summary>
    public class ODRHistoryWrapper
    {
        /// <summary>
        /// Gets or sets the Id of the request.
        /// </summary>
        [JsonPropertyName("uuid")]
        public Guid Id { get; set; } = default;

        /// <summary>
        /// Gets or sets the HDID of the requestor.
        /// </summary>
        [JsonPropertyName("hdid")]
        public string RequestorHDID { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the IP of the requestor.
        /// </summary>
        [JsonPropertyName("requestingIP")]
        public string RequestorIP { get; set; } = string.Empty;
    }
}
