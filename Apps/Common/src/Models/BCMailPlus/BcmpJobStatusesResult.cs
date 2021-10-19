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
    /// The response from BC Mail Plus for multiple job statuses.
    /// </summary>
    public class BcmpJobStatusesResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BcmpJobStatusesResult"/> class.
        /// </summary>
        public BcmpJobStatusesResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BcmpJobStatusesResult"/> class.
        /// </summary>
        /// <param name="statusResults">The list of job status results.</param>
        [JsonConstructor]
        public BcmpJobStatusesResult(IList<BcmpJobStatusResult> statusResults)
        {
            this.StatusResults = statusResults;
        }

        /// <summary>
        /// Gets the collection of job status results returned from the query.
        /// </summary>
        [JsonPropertyName("jobs")]
        public IList<BcmpJobStatusResult> StatusResults { get; } = new List<BcmpJobStatusResult>();
    }
}
