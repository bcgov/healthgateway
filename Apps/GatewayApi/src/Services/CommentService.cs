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
        private readonly IGatewayApiMappingService mappingService;
        private readonly ICommentDelegate commentDelegate;
        private readonly ILogger logger;
        private readonly IUserProfileDelegate profileDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="commentDelegate">Injected Comment delegate.</param>
        /// <param name="profileDelegate">Injected Profile delegate.</param>
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
                return RequestResultFactory.Error<UserComment>(ErrorType.InvalidState, validationResult.Errors);
            }

            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(userComment.UserProfileId, ct);
            string? key = profile?.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError("User does not have a key: {UserProfileId}", userComment.UserProfileId);
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.InvalidState, ServiceType.Database, "Profile Key not set");
            }

            Comment comment = this.mappingService.MapToComment(userComment, key);
            DbResult<Comment> dbResult = await this.commentDelegate.AddAsync(comment, ct: ct);

            if (dbResult.Status != DbStatusCode.Created)
            {
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message);
            }

            return RequestResultFactory.Success(this.mappingService.MapToUserComment(dbResult.Payload, key));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<UserComment>>> GetEntryCommentsAsync(string hdId, string parentEntryId, CancellationToken ct = default)
        {
            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(hdId, ct);
            string? key = profile?.EncryptionKey;

            // Check that the key has been set
            if (key == null)
            {
                this.logger.LogError("User does not have a key: {HdId}", hdId);
                return RequestResultFactory.ServiceError<IEnumerable<UserComment>>(ErrorType.InvalidState, ServiceType.Database, "Profile Key not set");
            }

            DbResult<IList<Comment>> dbComments = await this.commentDelegate.GetByParentEntryAsync(hdId, parentEntryId, ct);

            if (dbComments.Status != DbStatusCode.Read)
            {
                return RequestResultFactory.ServiceError<IEnumerable<UserComment>>(ErrorType.CommunicationInternal, ServiceType.Database, dbComments.Message);
            }

            return RequestResultFactory.Success(
                dbComments.Payload.Select(c => this.mappingService.MapToUserComment(c, key)),
                dbComments.Payload.Count,
                0,
                dbComments.Payload.Count);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IDictionary<string, IEnumerable<UserComment>>>> GetProfileCommentsAsync(string hdId, CancellationToken ct = default)
        {
            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(hdId, ct);
            string? key = profile?.EncryptionKey;

            // Check that the key has been set
            if (key == null)
            {
                this.logger.LogError("User does not have a key: {HdId}", hdId);
                return RequestResultFactory.ServiceError<IDictionary<string, IEnumerable<UserComment>>>(ErrorType.InvalidState, ServiceType.Database, "Profile Key not set");
            }

            DbResult<IEnumerable<Comment>> dbComments = await this.commentDelegate.GetAllAsync(hdId, ct);
            IEnumerable<UserComment> comments = dbComments.Payload.Select(c => this.mappingService.MapToUserComment(c, key));
            IDictionary<string, IEnumerable<UserComment>> userCommentsByEntry = comments.GroupBy(x => x.ParentEntryId).ToDictionary(g => g.Key, g => g.AsEnumerable());

            if (dbComments.Status != DbStatusCode.Read)
            {
                return RequestResultFactory.ServiceError<IDictionary<string, IEnumerable<UserComment>>>(ErrorType.CommunicationInternal, ServiceType.Database, dbComments.Message);
            }

            return RequestResultFactory.Success(
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
                return RequestResultFactory.Error<UserComment>(ErrorType.InvalidState, validationResult.Errors);
            }

            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(userComment.UserProfileId, ct);
            string? key = profile?.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError("User does not have a key: {UserProfileId}", userComment.UserProfileId);
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.InvalidState, ServiceType.Database, "Profile Key not set");
            }

            Comment comment = this.mappingService.MapToComment(userComment, key);
            DbResult<Comment> dbResult = await this.commentDelegate.UpdateAsync(comment, ct: ct);

            if (dbResult.Status != DbStatusCode.Updated)
            {
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message);
            }

            return RequestResultFactory.Success(this.mappingService.MapToUserComment(dbResult.Payload, key));
        }

        /// <inheritdoc/>
        public async Task<RequestResult<UserComment>> DeleteAsync(UserComment userComment, CancellationToken ct = default)
        {
            ValidationResult validationResult = await new UserCommentValidator().ValidateAsync(userComment, ct);

            if (!validationResult.IsValid)
            {
                return RequestResultFactory.Error<UserComment>(ErrorType.InvalidState, validationResult.Errors);
            }

            UserProfile? profile = await this.profileDelegate.GetUserProfileAsync(userComment.UserProfileId, ct);
            string? key = profile?.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError("User does not have a key: {UserProfileId}", userComment.UserProfileId);
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.InvalidState, ServiceType.Database, "Profile Key not set");
            }

            Comment comment = this.mappingService.MapToComment(userComment, key);
            DbResult<Comment> dbResult = await this.commentDelegate.DeleteAsync(comment, ct: ct);

            if (dbResult.Status != DbStatusCode.Deleted)
            {
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message);
            }

            return RequestResultFactory.Success(this.mappingService.MapToUserComment(dbResult.Payload, key));
        }
    }
}
