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
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation.Results;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class CommentService : ICommentService
    {
        private const string ProfileKeyNotSet = "Profile Key not set";
        private const string ProfileEncryptionKeyMissing = "Profile does not have an encryption key";

        private readonly IGatewayApiMappingService mappingService;
        private readonly ICommentDelegate commentDelegate;
        private readonly ILogger<CommentService> logger;
        private readonly IUserProfileDelegate profileDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="commentDelegate">The injected comment delegate.</param>
        /// <param name="profileDelegate">The injected profile delegate.</param>
        /// <param name="mappingService">The injected mapping service.</param>
        public CommentService(
            ILogger<CommentService> logger,
            ICommentDelegate commentDelegate,
            IUserProfileDelegate profileDelegate,
            IGatewayApiMappingService mappingService)
        {
            this.logger = logger;
            this.commentDelegate = commentDelegate;
            this.profileDelegate = profileDelegate;
            this.mappingService = mappingService;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserComment>> AddAsync(UserComment userComment, CancellationToken ct = default)
        {
            ValidationResult validationResult = await new UserCommentValidator().ValidateAsync(userComment, ct);

            if (!validationResult.IsValid)
            {
                this.logger.LogDebug("Comment did not pass validation");
                return RequestResultFactory.Error<UserComment>(ErrorType.InvalidState, validationResult.Errors);
            }

            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(userComment.UserProfileId, ct: ct);
            string? key = profile?.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError(ProfileEncryptionKeyMissing);
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.InvalidState, ServiceType.Database, ProfileKeyNotSet);
            }

            Comment comment = this.mappingService.MapToComment(userComment, key);
            DbResult<Comment> dbResult = await this.commentDelegate.AddAsync(comment, ct: ct);

            return dbResult.Status != DbStatusCode.Created
                ? RequestResultFactory.ServiceError<UserComment>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message)
                : RequestResultFactory.Success(this.mappingService.MapToUserComment(dbResult.Payload, key));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<UserComment>>> GetEntryCommentsAsync(string hdId, string parentEntryId, CancellationToken ct = default)
        {
            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(hdId, ct: ct);
            string? key = profile?.EncryptionKey;

            // Check that the key has been set
            if (key == null)
            {
                this.logger.LogError(ProfileEncryptionKeyMissing);
                return RequestResultFactory.ServiceError<IEnumerable<UserComment>>(ErrorType.InvalidState, ServiceType.Database, ProfileKeyNotSet);
            }

            DbResult<IList<Comment>> dbComments = await this.commentDelegate.GetByParentEntryAsync(hdId, parentEntryId, ct);

            return dbComments.Status != DbStatusCode.Read
                ? RequestResultFactory.ServiceError<IEnumerable<UserComment>>(ErrorType.CommunicationInternal, ServiceType.Database, dbComments.Message)
                : RequestResultFactory.Success(
                    dbComments.Payload.Select(c => this.mappingService.MapToUserComment(c, key)),
                    dbComments.Payload.Count,
                    0,
                    dbComments.Payload.Count);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IDictionary<string, IEnumerable<UserComment>>>> GetProfileCommentsAsync(string hdId, CancellationToken ct = default)
        {
            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(hdId, ct: ct);
            string? key = profile?.EncryptionKey;

            // Check that the key has been set
            if (key == null)
            {
                this.logger.LogError(ProfileEncryptionKeyMissing);
                return RequestResultFactory.ServiceError<IDictionary<string, IEnumerable<UserComment>>>(ErrorType.InvalidState, ServiceType.Database, ProfileKeyNotSet);
            }

            DbResult<IEnumerable<Comment>> dbComments = await this.commentDelegate.GetAllAsync(hdId, ct);
            IEnumerable<UserComment> comments = dbComments.Payload.Select(c => this.mappingService.MapToUserComment(c, key));
            Dictionary<string, IEnumerable<UserComment>> userCommentsByEntry = comments.GroupBy(x => x.ParentEntryId).ToDictionary(g => g.Key, g => g.AsEnumerable());

            return dbComments.Status != DbStatusCode.Read
                ? RequestResultFactory.ServiceError<IDictionary<string, IEnumerable<UserComment>>>(ErrorType.CommunicationInternal, ServiceType.Database, dbComments.Message)
                : RequestResultFactory.Success<IDictionary<string, IEnumerable<UserComment>>>(
                    userCommentsByEntry,
                    userCommentsByEntry.Count,
                    0,
                    userCommentsByEntry.Count);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserComment>> UpdateAsync(UserComment userComment, CancellationToken ct = default)
        {
            ValidationResult validationResult = await new UserCommentValidator().ValidateAsync(userComment, ct);

            if (!validationResult.IsValid)
            {
                this.logger.LogDebug("Comment did not pass validation");
                return RequestResultFactory.Error<UserComment>(ErrorType.InvalidState, validationResult.Errors);
            }

            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(userComment.UserProfileId, ct: ct);
            string? key = profile?.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError(ProfileEncryptionKeyMissing);
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.InvalidState, ServiceType.Database, ProfileKeyNotSet);
            }

            Comment comment = this.mappingService.MapToComment(userComment, key);
            DbResult<Comment> dbResult = await this.commentDelegate.UpdateAsync(comment, ct: ct);

            return dbResult.Status != DbStatusCode.Updated
                ? RequestResultFactory.ServiceError<UserComment>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message)
                : RequestResultFactory.Success(this.mappingService.MapToUserComment(dbResult.Payload, key));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserComment>> DeleteAsync(UserComment userComment, CancellationToken ct = default)
        {
            ValidationResult validationResult = await new UserCommentValidator().ValidateAsync(userComment, ct);

            if (!validationResult.IsValid)
            {
                this.logger.LogDebug("Comment did not pass validation");
                return RequestResultFactory.Error<UserComment>(ErrorType.InvalidState, validationResult.Errors);
            }

            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(userComment.UserProfileId, ct: ct);
            string? key = profile?.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError(ProfileEncryptionKeyMissing);
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.InvalidState, ServiceType.Database, ProfileKeyNotSet);
            }

            Comment comment = this.mappingService.MapToComment(userComment, key);
            DbResult<Comment> dbResult = await this.commentDelegate.DeleteAsync(comment, ct: ct);

            return dbResult.Status != DbStatusCode.Deleted
                ? RequestResultFactory.ServiceError<UserComment>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message)
                : RequestResultFactory.Success(this.mappingService.MapToUserComment(dbResult.Payload, key));
        }
    }
}
