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
        /// <returns>An IEnumerable of Comments wrapped in a DBResult.</returns>
        DbResult<IEnumerable<Comment>> GetByParentEntry(string hdId, string parentEntryId);

        /// <summary>
        /// Add the given note.
        /// </summary>
        /// <param name="comment">The comment to be added to the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A comment wrapped in a DBResult.</returns>
        DbResult<Comment> Add(Comment comment, bool commit = true);

        /// <summary>
        /// Update the supplied note.
        /// </summary>
        /// <param name="comment">The comment to be updated in the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A comment wrapped in a DBResult.</returns>
        DbResult<Comment> Update(Comment comment, bool commit = true);

        /// <summary>
        /// Deletes the supplied note.
        /// </summary>
        /// <param name="comment">The comment to be deleted in the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A comment wrapped in a DBResult.</returns>
        DbResult<Comment> Delete(Comment comment, bool commit = true);

        /// <summary>
        /// Gets a list of comments ordered by the created datetime for the given HdId.
        /// </summary>
        /// <param name="hdId">The users health identifier id.</param>
        /// <returns>An IEnumerable of Comments wrapped in a DBResult.</returns>
        DbResult<IEnumerable<Comment>> GetAll(string hdId);

        /// <summary>
        /// Gets a list of all the comments ordered by the CreatedDateTime in assending order.
        /// </summary>
        /// <param name="page">The starting offset for the query.</param>
        /// <param name="pageSize">The maximum amount of rows to return.</param>
        /// <returns>A list of comments wrapped in a DBResult.</returns>
        DbResult<IEnumerable<Comment>> GetAll(int page, int pageSize);
    }
}
