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
namespace HealthGateway.GatewayApi.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class CommentService : ICommentService
    {
        private readonly ILogger logger;
        private readonly ICommentDelegate commentDelegate;
        private readonly IUserProfileDelegate profileDelegate;
        private readonly ICryptoDelegate cryptoDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="commentDelegate">Injected Comment delegate.</param>
        /// <param name="profileDelegate">Injected Profile delegate.</param>
        /// <param name="cryptoDelegate">Injected Crypto delegate.</param>
        public CommentService(ILogger<CommentService> logger, ICommentDelegate commentDelegate, IUserProfileDelegate profileDelegate, ICryptoDelegate cryptoDelegate)
        {
            this.logger = logger;
            this.commentDelegate = commentDelegate;
            this.profileDelegate = profileDelegate;
            this.cryptoDelegate = cryptoDelegate;
        }

        /// <inheritdoc />
        public RequestResult<UserComment> Add(UserComment userComment)
        {
            UserProfile profile = this.profileDelegate.GetUserProfile(userComment.UserProfileId).Payload;
            string? key = profile.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError($"User does not have a key: ${userComment.UserProfileId}");
                return new RequestResult<UserComment>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Profile Key not set", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
                };
            }

            Comment comment = userComment.ToDbModel(this.cryptoDelegate, key);

            DBResult<Comment> dbComment = this.commentDelegate.Add(comment);
            RequestResult<UserComment> result = new RequestResult<UserComment>()
            {
                ResourcePayload = UserComment.CreateFromDbModel(dbComment.Payload, this.cryptoDelegate, key),
                ResultStatus = dbComment.Status == DBStatusCode.Created ? ResultType.Success : ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = dbComment.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
            };
            return result;
        }

        /// <inheritdoc />
        public RequestResult<IEnumerable<UserComment>> GetEntryComments(string hdId, string parentEntryId)
        {
            UserProfile profile = this.profileDelegate.GetUserProfile(hdId).Payload;
            string? key = profile.EncryptionKey;

            // Check that the key has been set
            if (key == null)
            {
                this.logger.LogError($"User does not have a key: ${hdId}");
                return new RequestResult<IEnumerable<UserComment>>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Profile Key not set", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
                };
            }

            DBResult<IEnumerable<Comment>> dbComments = this.commentDelegate.GetByParentEntry(hdId, parentEntryId);
            RequestResult<IEnumerable<UserComment>> result = new RequestResult<IEnumerable<UserComment>>()
            {
                ResourcePayload = UserComment.CreateListFromDbModel(dbComments.Payload, this.cryptoDelegate, key),
                TotalResultCount = dbComments.Payload.Count(),
                PageIndex = 0,
                PageSize = dbComments.Payload.Count(),
                ResultStatus = dbComments.Status == DBStatusCode.Read ? ResultType.Success : ResultType.Error,
                ResultError = dbComments.Status != DBStatusCode.Read ? new RequestResultError() { ResultMessage = dbComments.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) } : null,
            };
            return result;
        }

        /// <inheritdoc />
        public RequestResult<IDictionary<string, IEnumerable<UserComment>>> GetProfileComments(string hdId)
        {
            UserProfile profile = this.profileDelegate.GetUserProfile(hdId).Payload;
            string? key = profile.EncryptionKey;

            // Check that the key has been set
            if (key == null)
            {
                this.logger.LogError($"User does not have a key: ${hdId}");
                return new RequestResult<IDictionary<string, IEnumerable<UserComment>>>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Profile Key not set", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
                };
            }

            DBResult<IEnumerable<Comment>> dbComments = this.commentDelegate.GetAll(hdId);
            IEnumerable<UserComment> comments = UserComment.CreateListFromDbModel(dbComments.Payload, this.cryptoDelegate, key);

            IDictionary<string, IEnumerable<UserComment>> userCommentsByEntry = comments.GroupBy(x => x.ParentEntryId).ToDictionary(g => g.Key, g => g.AsEnumerable());

            RequestResult<IDictionary<string, IEnumerable<UserComment>>> result = new RequestResult<IDictionary<string, IEnumerable<UserComment>>>()
            {
                ResourcePayload = userCommentsByEntry,
                TotalResultCount = userCommentsByEntry.Count,
                PageIndex = 0,
                PageSize = userCommentsByEntry.Count,
                ResultStatus = dbComments.Status == DBStatusCode.Read ? ResultType.Success : ResultType.Error,
                ResultError = dbComments.Status != DBStatusCode.Read ? new RequestResultError() { ResultMessage = dbComments.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) } : null,
            };
            return result;
        }

        /// <inheritdoc />
        public RequestResult<UserComment> Update(UserComment userComment)
        {
            UserProfile profile = this.profileDelegate.GetUserProfile(userComment.UserProfileId).Payload;
            string? key = profile.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError($"User does not have a key: ${userComment.UserProfileId}");
                return new RequestResult<UserComment>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Profile Key not set", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
                };
            }

            Comment comment = userComment.ToDbModel(this.cryptoDelegate, key);

            DBResult<Comment> dbResult = this.commentDelegate.Update(comment);
            RequestResult<UserComment> result = new RequestResult<UserComment>()
            {
                ResourcePayload = UserComment.CreateFromDbModel(dbResult.Payload, this.cryptoDelegate, key),
                ResultStatus = dbResult.Status == DBStatusCode.Updated ? ResultType.Success : ResultType.Error,
                ResultError = dbResult.Status != DBStatusCode.Updated ? new RequestResultError() { ResultMessage = dbResult.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) } : null,
            };
            return result;
        }

        /// <inheritdoc />
        public RequestResult<UserComment> Delete(UserComment userComment)
        {
            UserProfile profile = this.profileDelegate.GetUserProfile(userComment.UserProfileId).Payload;
            string? key = profile.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError($"User does not have a key: ${userComment.UserProfileId}");
                return new RequestResult<UserComment>()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Profile Key not set", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
                };
            }

            Comment comment = userComment.ToDbModel(this.cryptoDelegate, key);

            DBResult<Comment> dbResult = this.commentDelegate.Delete(comment);
            RequestResult<UserComment> result = new RequestResult<UserComment>()
            {
                ResourcePayload = UserComment.CreateFromDbModel(dbResult.Payload, this.cryptoDelegate, key),
                ResultStatus = dbResult.Status == DBStatusCode.Deleted ? ResultType.Success : ResultType.Error,
                ResultError = dbResult.Status != DBStatusCode.Deleted ? new RequestResultError() { ResultMessage = dbResult.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) } : null,
            };
            return result;
        }
    }
}
