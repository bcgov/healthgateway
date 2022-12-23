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
    using AutoMapper;
    using FluentValidation.Results;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.MapUtils;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Validations;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class CommentService : ICommentService
    {
        private readonly IMapper autoMapper;
        private readonly ICommentDelegate commentDelegate;
        private readonly ICryptoDelegate cryptoDelegate;
        private readonly ILogger logger;
        private readonly IUserProfileDelegate profileDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="commentDelegate">Injected Comment delegate.</param>
        /// <param name="profileDelegate">Injected Profile delegate.</param>
        /// <param name="cryptoDelegate">Injected Crypto delegate.</param>
        /// <param name="autoMapper">The inject automapper provider.</param>
        public CommentService(ILogger<CommentService> logger, ICommentDelegate commentDelegate, IUserProfileDelegate profileDelegate, ICryptoDelegate cryptoDelegate, IMapper autoMapper)
        {
            this.logger = logger;
            this.commentDelegate = commentDelegate;
            this.profileDelegate = profileDelegate;
            this.cryptoDelegate = cryptoDelegate;
            this.autoMapper = autoMapper;
        }

        /// <inheritdoc/>
        public RequestResult<UserComment> Add(UserComment userComment)
        {
            ValidationResult validationResult = new UserCommentValidator().Validate(userComment);

            if (!validationResult.IsValid)
            {
                return RequestResultFactory.Error<UserComment>(ErrorType.InvalidState, validationResult.Errors);
            }

            UserProfile profile = this.profileDelegate.GetUserProfile(userComment.UserProfileId).Payload;
            string? key = profile.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError("User does not have a key: {UserProfileId}", userComment.UserProfileId);
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.InvalidState, ServiceType.Database, "Profile Key not set");
            }

            Comment comment = CommentMapUtils.ToDbModel(userComment, this.cryptoDelegate, key, this.autoMapper);
            DbResult<Comment> dbResult = this.commentDelegate.Add(comment);

            if (dbResult.Status != DbStatusCode.Created)
            {
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message);
            }

            return RequestResultFactory.Success(CommentMapUtils.CreateFromDbModel(dbResult.Payload, this.cryptoDelegate, key, this.autoMapper));
        }

        /// <inheritdoc/>
        public RequestResult<IEnumerable<UserComment>> GetEntryComments(string hdId, string parentEntryId)
        {
            UserProfile profile = this.profileDelegate.GetUserProfile(hdId).Payload;
            string? key = profile.EncryptionKey;

            // Check that the key has been set
            if (key == null)
            {
                this.logger.LogError("User does not have a key: {HdId}", hdId);
                return RequestResultFactory.ServiceError<IEnumerable<UserComment>>(ErrorType.InvalidState, ServiceType.Database, "Profile Key not set");
            }

            DbResult<IEnumerable<Comment>> dbComments = this.commentDelegate.GetByParentEntry(hdId, parentEntryId);

            if (dbComments.Status != DbStatusCode.Read)
            {
                return RequestResultFactory.ServiceError<IEnumerable<UserComment>>(ErrorType.CommunicationInternal, ServiceType.Database, dbComments.Message);
            }

            return RequestResultFactory.Success(
                dbComments.Payload.Select(c => CommentMapUtils.CreateFromDbModel(c, this.cryptoDelegate, key, this.autoMapper)),
                dbComments.Payload.Count(),
                0,
                dbComments.Payload.Count());
        }

        /// <inheritdoc/>
        public RequestResult<IDictionary<string, IEnumerable<UserComment>>> GetProfileComments(string hdId)
        {
            UserProfile profile = this.profileDelegate.GetUserProfile(hdId).Payload;
            string? key = profile.EncryptionKey;

            // Check that the key has been set
            if (key == null)
            {
                this.logger.LogError("User does not have a key: {HdId}", hdId);
                return RequestResultFactory.ServiceError<IDictionary<string, IEnumerable<UserComment>>>(ErrorType.InvalidState, ServiceType.Database, "Profile Key not set");
            }

            DbResult<IEnumerable<Comment>> dbComments = this.commentDelegate.GetAll(hdId);
            IEnumerable<UserComment> comments = dbComments.Payload.Select(c => CommentMapUtils.CreateFromDbModel(c, this.cryptoDelegate, key, this.autoMapper));
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
        public RequestResult<UserComment> Update(UserComment userComment)
        {
            ValidationResult validationResult = new UserCommentValidator().Validate(userComment);

            if (!validationResult.IsValid)
            {
                return RequestResultFactory.Error<UserComment>(ErrorType.InvalidState, validationResult.Errors);
            }

            UserProfile profile = this.profileDelegate.GetUserProfile(userComment.UserProfileId).Payload;
            string? key = profile.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError("User does not have a key: {UserProfileId}", userComment.UserProfileId);
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.InvalidState, ServiceType.Database, "Profile Key not set");
            }

            Comment comment = CommentMapUtils.ToDbModel(userComment, this.cryptoDelegate, key, this.autoMapper);
            DbResult<Comment> dbResult = this.commentDelegate.Update(comment);

            if (dbResult.Status != DbStatusCode.Updated)
            {
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message);
            }

            return RequestResultFactory.Success(CommentMapUtils.CreateFromDbModel(dbResult.Payload, this.cryptoDelegate, key, this.autoMapper));
        }

        /// <inheritdoc/>
        public RequestResult<UserComment> Delete(UserComment userComment)
        {
            ValidationResult validationResult = new UserCommentValidator().Validate(userComment);

            if (!validationResult.IsValid)
            {
                return RequestResultFactory.Error<UserComment>(ErrorType.InvalidState, validationResult.Errors);
            }

            UserProfile profile = this.profileDelegate.GetUserProfile(userComment.UserProfileId).Payload;
            string? key = profile.EncryptionKey;
            if (key == null)
            {
                this.logger.LogError("User does not have a key: {UserProfileId}", userComment.UserProfileId);
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.InvalidState, ServiceType.Database, "Profile Key not set");
            }

            Comment comment = CommentMapUtils.ToDbModel(userComment, this.cryptoDelegate, key, this.autoMapper);
            DbResult<Comment> dbResult = this.commentDelegate.Delete(comment);

            if (dbResult.Status != DbStatusCode.Deleted)
            {
                return RequestResultFactory.ServiceError<UserComment>(ErrorType.CommunicationInternal, ServiceType.Database, dbResult.Message);
            }

            return RequestResultFactory.Success(CommentMapUtils.CreateFromDbModel(dbResult.Payload, this.cryptoDelegate, key, this.autoMapper));
        }
    }
}
