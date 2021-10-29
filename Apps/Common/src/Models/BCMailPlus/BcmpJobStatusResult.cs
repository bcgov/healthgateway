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
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Constants;

    /// <summary>
    /// The response from BC Mail Plus for a single job status.
    /// </summary>
    public class BcmpJobStatusResult
    {
        /// <summary>
        /// Gets or sets the unique Job ID associated with the Vaccine Proof.
        /// </summary>
        [JsonPropertyName("jobId")]
        public string JobId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status of the Vaccine Proof.
        /// </summary>
        [JsonPropertyName("jobStatus")]
        public BcmpJobStatus JobStatus { get; set; } = BcmpJobStatus.Error;

        /// <summary>
        /// Gets or sets the additional job properties.
        /// </summary>
        [JsonPropertyName("jobProperties")]
        public BcmpProperties JobProperties { get; set; } = new();

        /// <summary>
        /// Gets or sets the errors associated with the Vaccine Proof.
        /// </summary>
        [JsonPropertyName("errors")]
        public string Errors { get; set; } = string.Empty;
    }
}
