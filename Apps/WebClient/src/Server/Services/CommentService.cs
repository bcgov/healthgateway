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
namespace HealthGateway.WebClient.Services
{
    using System.Collections.Generic;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class CommentService : ICommentService
    {
        private readonly ILogger logger;
        private readonly ICommentDelegate commentDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="commentDelegate">Injected Comment delegate.</param>
        public CommentService(ILogger<CommentService> logger, ICommentDelegate commentDelegate)
        {
            this.logger = logger;
            this.commentDelegate = commentDelegate;
        }

        /// <inheritdoc />
        public RequestResult<Comment> Add(Comment comment)
        {
            DBResult<Comment> dbComment = this.commentDelegate.Add(comment);
            RequestResult<Comment> result = new RequestResult<Comment>()
            {
                ResourcePayload = dbComment.Payload,
                ResultStatus = dbComment.Status == Database.Constant.DBStatusCode.Created ? Common.Constants.ResultType.Success : Common.Constants.ResultType.Error,
                ResultMessage = dbComment.Message,
            };
            return result;

        }

        /// <inheritdoc />
        public RequestResult<IEnumerable<Comment>> GetList(string hdId, string eventId)
        {
            DBResult<IEnumerable<Comment>> dbComments = this.commentDelegate.GetList(hdId, eventId);
            RequestResult<IEnumerable<Comment>> result = new RequestResult<IEnumerable<Comment>>()
            {
                ResourcePayload = dbComments.Payload,
                ResultStatus = dbComments.Status == Database.Constant.DBStatusCode.Read ? Common.Constants.ResultType.Success : Common.Constants.ResultType.Error,
                ResultMessage = dbComments.Message,
            };
            return result;

        }

        /// <inheritdoc />
        public RequestResult<Comment> Update(Comment comment)
        {
            DBResult<Comment> dbResult = this.commentDelegate.Update(comment);
            RequestResult<Comment> result = new RequestResult<Comment>()
            {
                ResourcePayload = dbResult.Payload,
                ResultStatus = dbResult.Status == Database.Constant.DBStatusCode.Updated ? Common.Constants.ResultType.Success : Common.Constants.ResultType.Error,
                ResultMessage = dbResult.Message,
            };
            return result;
        }

        /// <inheritdoc />
        public RequestResult<Comment> Delete(Comment comment)
        {
            DBResult<Comment> dbResult = this.commentDelegate.Delete(comment);
            RequestResult<Comment> result = new RequestResult<Comment>()
            {
                ResourcePayload = dbResult.Payload,
                ResultStatus = dbResult.Status == Database.Constant.DBStatusCode.Deleted ? Common.Constants.ResultType.Success : Common.Constants.ResultType.Error,
                ResultMessage = dbResult.Message,
            };
            return result;
        }
    }
}
