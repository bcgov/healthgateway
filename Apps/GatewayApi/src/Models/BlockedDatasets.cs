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
namespace HealthGateway.GatewayApi.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// Model representing blocked datasets for a specified user.
    /// </summary>
    public class BlockedDatasets
    {
        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        public string Hdid { get; set; } = null!;

        /// <summary>
        /// Gets or sets the user's data sources that are blocked.
        /// </summary>
        [JsonPropertyName("blockedDataSources")]
        public IEnumerable<DataSource> DataSources { get; set; } = [];
    }
}
