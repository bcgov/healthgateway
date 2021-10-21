//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Common.Models.BCMailPlus
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The BC Mail Plus asset query model.
    /// </summary>
    public class BcmpAssetQuery
    {
        /// <summary>
        /// Gets or sets the ID of the job corresponding to the asset to retrieve.
        /// </summary>
        [JsonPropertyName("job")]
        public string JobId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the asset type that should be retrieved.
        /// </summary>
        [JsonPropertyName("asset")]
        public string AssetType { get; set; } = string.Empty;
    }
}
