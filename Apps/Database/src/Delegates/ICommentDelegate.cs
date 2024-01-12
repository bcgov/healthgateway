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
namespace HealthGateway.Database.Delegates
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Operations to be performed for Comments.
    /// </summary>
    public interface ICommentDelegate
    {
        /// <summary>
        /// Gets a list of comments ordered by the created datetime for the given HdId and event Id.
        /// </summary>
        /// <param name="hdId">The users health identifier id.</param>
        /// <param name="parentEntryId">The parent entry id.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>An IList of Comments wrapped in a DBResult.</returns>
        Task<DbResult<IList<Comment>>> GetByParentEntryAsync(string hdId, string parentEntryId, CancellationToken ct = default);

        /// <summary>
        /// Add the given note.
        /// </summary>
        /// <param name="comment">The comment to be added to the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A comment wrapped in a DBResult.</returns>
        Task<DbResult<Comment>> AddAsync(Comment comment, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Update the supplied note.
        /// </summary>
        /// <param name="comment">The comment to be updated in the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A comment wrapped in a DBResult.</returns>
        Task<DbResult<Comment>> UpdateAsync(Comment comment, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Deletes the supplied note.
        /// </summary>
        /// <param name="comment">The comment to be deleted in the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A comment wrapped in a DBResult.</returns>
        Task<DbResult<Comment>> DeleteAsync(Comment comment, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Gets a list of comments ordered by the created datetime for the given HdId.
        /// </summary>
        /// <param name="hdId">The users health identifier id.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>An IEnumerable of Comments wrapped in a DBResult.</returns>
        Task<DbResult<IEnumerable<Comment>>> GetAllAsync(string hdId, CancellationToken ct = default);

        /// <summary>
        /// Gets a list of all the comments ordered by the CreatedDateTime in ascending order.
        /// </summary>
        /// <param name="page">The starting offset for the query.</param>
        /// <param name="pageSize">The maximum amount of rows to return.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A list of comments.</returns>
        Task<IList<Comment>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    }
}
