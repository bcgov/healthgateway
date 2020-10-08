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
namespace HealthGateway.Encounter.Models.ODR
{
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Models.ODR;

    /// <summary>
    /// Object that defines the MSP Visit History Request/Response model.
    /// </summary>
    public class MSPVisitHistory : ODRHistoryWrapper
    {
        /// <summary>
        /// Gets or sets the ODRHistoryQuery for the MSPVisitHistory integration.
        /// </summary>
        [JsonPropertyName("getMspVisitHistoryRequest")]
        public ODRHistoryQuery? Query { get; set; }

        /// <summary>
        /// Gets or sets the MSPVisitHistoryResponse for the MSPVisitHistory integration.
        /// </summary>
        [JsonPropertyName("getMspVisitHistoryResponse")]
        public MSPVisitHistoryResponse? Response { get; set; }
    }
}
