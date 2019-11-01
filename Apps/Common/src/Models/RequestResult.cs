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
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Class that represents the result of a request. Contains members for handling pagination and error resolution.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    public class RequestResult<T> : ActionResult
    {
        /// <summary>
        /// Gets or sets the result payload.
        /// </summary>
        public T ResourcePayload { get; set; }

        /// <summary>
        /// Gets or sets the total result count for the request for pagination.
        /// </summary>
        public int TotalResultCount { get; set; }

        /// <summary>
        /// Gets or sets the page being returned on this result for pagination.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Gets or sets the page size for pagination.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the error message if there was any.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}