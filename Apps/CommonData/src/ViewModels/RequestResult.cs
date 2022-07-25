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
namespace HealthGateway.Common.Data.ViewModels
{
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// Class that represents the result of a request. Contains members for handling pagination and error resolution.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    [ExcludeFromCodeCoverage]
    public class RequestResult<T>
        where T : class?
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResult{T}"/> class.
        /// </summary>
        public RequestResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResult{T}"/> class.
        /// </summary>
        /// <param name="resourcePayload">The resource payload.</param>
        /// <param name="resultStatus">The result status.</param>
        public RequestResult(
            T resourcePayload,
            ResultType resultStatus)
        {
            this.ResourcePayload = resourcePayload;
            this.ResultStatus = resultStatus;
        }

        /// <summary>
        /// Gets or sets the result payload.
        /// </summary>
        [JsonPropertyName("resourcePayload")]
        public T? ResourcePayload { get; set; }

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
