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
namespace HealthGateway.Common.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The response from the BCMailPlusVaccineProofDelegate.
    /// </summary>
    public class BcMailPlusResponse
    {
        /// <summary>
        /// Gets or sets the unique id for the request to BC Mail Plus.
        /// </summary>
        [JsonPropertyName("jobId")]
        public string Id { get; set; } = null!;

        /// <summary>
        /// Gets or sets the status of the job at BC Mail Plus.
        /// </summary>
        [JsonPropertyName("jobStatus")]
        public string Status { get; set; } = null!;
    }
}
