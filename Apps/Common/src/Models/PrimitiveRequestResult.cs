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
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Class that represents the result of a request with a primitive values.
    /// Contains members for handling pagination and error resolution.
    /// Note: Will not be necessary on the next release of c#.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    public class PrimitiveRequestResult<T>
        where T : struct
    {
        /// <summary>
        /// Gets or sets the result payload.
        /// </summary>
        [JsonPropertyName("resourcePayload")]
        public T ResourcePayload { get; set; } = default(T);

        /// <summary>
        /// Gets or sets the total result count for the request for pagination.
        /// </summary>
        [JsonPropertyName("totalResultCount")]
        public int? TotalResultCount { get; set; }

        /// <summary>
        /// Gets or sets the page being returned on this result for pagination.
        /// </summary>
        [JsonPropertyName("pageIndex")]
        public int? PageIndex { get; set; }

        /// <summary>
        /// Gets or sets the page size for pagination.
        /// </summary>
        [JsonPropertyName("pageSize")]
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets the Result of the request.
        /// </summary>
        [JsonPropertyName("resultStatus")]
        public ResultType ResultStatus { get; set; }

        /// <summary>
        /// Gets or sets the ResultError of the request. Can be null.
        /// </summary>
        [JsonPropertyName("resultError")]
        public RequestResultError? ResultError { get; set; } = null;
    }
}
