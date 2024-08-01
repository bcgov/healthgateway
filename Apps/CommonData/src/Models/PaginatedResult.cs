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
namespace HealthGateway.Common.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Wrapper for the result of a request that uses pagination.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    public class PaginatedResult<T>
    {
        /// <summary>
        /// Gets the subset of data belonging to the page requested.
        /// </summary>
        public required IList<T> Data { get; init; }

        /// <summary>
        /// Gets the index of the page requested.
        /// </summary>
        public required int PageIndex { get; init; }

        /// <summary>
        /// Gets the size of the page requested.
        /// </summary>
        public required int PageSize { get; init; }

        /// <summary>
        /// Gets the count of all items, not limited to those on this page.
        /// </summary>
        public required int TotalCount { get; init; }

        /// <summary>
        /// Transforms paginated result of type <typeparamref name="T"/> into paginated result of type
        /// <typeparamref name="TOutput"/>.
        /// </summary>
        /// <param name="transformFunction">
        /// Function that transforms instances of type <typeparamref name="T"/> into instances of
        /// type <typeparamref name="TOutput"/>.
        /// </param>
        /// <typeparam name="TOutput">The output type.</typeparam>
        /// <returns>The transformed paginated result.</returns>
        public PaginatedResult<TOutput> Transform<TOutput>(Func<T, TOutput> transformFunction)
        {
            return new()
            {
                Data = this.Data.Select(transformFunction).ToList(),
                PageIndex = this.PageIndex,
                PageSize = this.PageSize,
                TotalCount = this.TotalCount,
            };
        }
    }
}
