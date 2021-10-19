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
    /// The BC Mail Plus status query model.
    /// </summary>
    public class BcmpJobStatusesQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BcmpJobStatusesQuery"/> class.
        /// </summary>
        public BcmpJobStatusesQuery()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BcmpJobStatusesQuery"/> class.
        /// </summary>
        /// <param name="jobIds">The list of job IDs to check.</param>
        [JsonConstructor]
        public BcmpJobStatusesQuery(IList<string> jobIds)
        {
            this.JobIds = jobIds;
        }

        /// <summary>
        /// Gets the job IDs for the job statuses to retrieve.
        /// </summary>
        [JsonPropertyName("jobs")]
        public IList<string> JobIds { get; } = new List<string>();
    }
}
