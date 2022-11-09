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
namespace HealthGateway.Database.Wrapper
{
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Database.Constants;

    /// <summary>
    /// Utilities for DBDelegate including paging a LINQ Query.
    /// </summary>
    public static class DbDelegateHelper
    {
        /// <summary>
        /// Gets a list of DBModel records for a specific page.
        /// </summary>
        /// <param name="query">A query that needs to be paged.</param>
        /// <param name="page">The starting offset for the query.</param>
        /// <param name="pageSize">The maximum amount of rows to return.</param>
        /// <typeparam name="T">A DBModel type.</typeparam>
        /// <returns>A list of DBModel records wrapped in a DBResult.</returns>
        public static DbResult<IEnumerable<T>> GetPagedDbResult<T>(IQueryable<T> query, int page, int pageSize)
            where T : class
        {
            int offset = page * pageSize;
            DbResult<IEnumerable<T>> result = new()
            {
                Payload = query.Skip(offset).Take(pageSize).ToList(),
            };
            result.Status = DbStatusCode.Read;
            return result;
        }
    }
}
