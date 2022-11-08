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
namespace HealthGateway.GatewayApi.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.MapUtils;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApi.Test.Services.Utils;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// CommentService's Unit Tests.
    /// </summary>
    public class CommentServiceTests
    {
        private readonly string hdid = "1234567890123456789012345678901234567890123456789012";
        private readonly string parentEntryId = "123456789";

        /// <summary>
        /// GetComments - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetComments()
        {
            (RequestResult<IEnumerable<UserComment>> actualResult, List<UserComment> userCommentList) = this.ExecuteGetComments("abc");

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            userCommentList.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetComments - Database error.
        /// </summary>
        [Fact]
        public void ShouldGetCommentsWithDbError()
        {
            (RequestResult<IEnumerable<UserComment>> actualResult, _) = this.ExecuteGetComments("abc", DbStatusCode.Error);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.True(actualResult?.ResultError?.ErrorCode.EndsWith("-CI-DB", StringComparison.InvariantCulture));
        }

        /// <summary>
        /// InsertComment - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldInsertComment()
        {
            (RequestResult<UserComment> actualResult, UserComment createdRecord) = this.ExecuteInsertComment();

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            createdRecord.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// InsertComment - Database Error.
        /// </summary>
        [Fact]
        public void ShouldInsertCommentWithDBError()
        {
            (RequestResult<UserComment> actualResult, _) = this.ExecuteInsertComment(DbStatusCode.Error);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        /// <summary>
        /// UpdateComment - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldUpdateComment()
        {
            (RequestResult<UserComment> actualResult, UserComment updatedRecord) = this.ExecuteUpdateComment();

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            updatedRecord.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// UpdateComment - Database Error.
        /// </summary>
        [Fact]
        public void ShouldUpdateCommentWithDBError()
        {
            (RequestResult<UserComment> actualResult, _) = this.ExecuteUpdateComment(DbStatusCode.Error);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        /// <summary>
        /// DeleteComment - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldDeleteComment()
        {
            (RequestResult<UserComment> actualResult, UserComment deletedRecord) = this.ExecuteDeleteComment();

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            deletedRecord.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// DeleteComment - Database Error.
        /// </summary>
        [Fact]
        public void ShouldDeleteCommentWithDBError()
        {
            (RequestResult<UserComment> actualResult, _) = this.ExecuteDeleteComment(DbStatusCode.Error);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        /// <summary>
        /// GetComments - No Encryption key error.
        /// </summary>
        [Fact]
        public void ShouldGetCommentsWithNoKeyError()
        {
            string? encryptionKey = null;
            DbResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                new Mock<ICommentDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                MapperUtil.InitializeAutoMapper());

            RequestResult<IEnumerable<UserComment>> actualResult = service.GetEntryComments(this.hdid, this.parentEntryId);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// InsertComment - No Encryption key error.
        /// </summary>
        [Fact]
        public void ShouldInsertCommentWithNoKeyError()
        {
            string? encryptionKey = null;
            DbResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            UserComment userComment = new()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1),
            };

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                new Mock<ICommentDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                MapperUtil.InitializeAutoMapper());

            RequestResult<UserComment> actualResult = service.Add(userComment);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// UpdateComment - No Encryption key error.
        /// </summary>
        [Fact]
        public void ShouldUpdateCommentWithNoKeyError()
        {
            string? encryptionKey = null;
            DbResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            UserComment userComment = new()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1),
            };

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                new Mock<ICommentDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                MapperUtil.InitializeAutoMapper());

            RequestResult<UserComment> actualResult = service.Update(userComment);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// DeleteComment - No Encryption key error.
        /// </summary>
        [Fact]
        public void ShouldDeleteCommentWithNoKeyError()
        {
            string? encryptionKey = null;
            DbResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            UserComment userComment = new()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1),
            };

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                new Mock<ICommentDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                MapperUtil.InitializeAutoMapper());

            RequestResult<UserComment> actualResult = service.Delete(userComment);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        private (RequestResult<UserComment> ActualResult, UserComment UserComment) ExecuteDeleteComment(DbStatusCode dBStatusCode = DbStatusCode.Deleted)
        {
            string encryptionKey = "abc";
            DbResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserComment userComment = new()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1),
            };
            IMapper autoMapper = MapperUtil.InitializeAutoMapper();
            Comment comment = CommentMapUtils.ToDbModel(userComment, cryptoDelegateMock.Object, encryptionKey, autoMapper);

            DbResult<Comment> deleteResult = new()
            {
                Payload = comment,
                Status = dBStatusCode,
            };

            Mock<ICommentDelegate> commentDelegateMock = new();
            commentDelegateMock.Setup(s => s.Delete(It.Is<Comment>(x => x.Text == comment.Text), true)).Returns(deleteResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                autoMapper);

            RequestResult<UserComment> actualResult = service.Delete(userComment);
            return (actualResult, userComment);
        }

        private (RequestResult<UserComment> ActualResult, UserComment UserComment) ExecuteUpdateComment(DbStatusCode dBStatusCode = DbStatusCode.Updated)
        {
            string encryptionKey = "abc";
            DbResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserComment userComment = new()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Updated Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1),
            };

            IMapper autoMapper = MapperUtil.InitializeAutoMapper();
            Comment comment = CommentMapUtils.ToDbModel(userComment, cryptoDelegateMock.Object, encryptionKey, autoMapper);

            DbResult<Comment> updateResult = new()
            {
                Payload = comment,
                Status = dBStatusCode,
            };

            Mock<ICommentDelegate> commentDelegateMock = new();
            commentDelegateMock.Setup(s => s.Update(It.Is<Comment>(x => x.Text == comment.Text), true)).Returns(updateResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                autoMapper);

            RequestResult<UserComment> actualResult = service.Update(userComment);
            return (actualResult, userComment);
        }

        private (RequestResult<IEnumerable<UserComment>> ActualResult, List<UserComment> UserCommentList) ExecuteGetComments(
            string? encryptionKey = null,
            DbStatusCode dbResultStatus = DbStatusCode.Read)
        {
            DbResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            List<Comment> commentList = new()
            {
                new Comment
                {
                    UserProfileId = this.hdid,
                    ParentEntryId = this.parentEntryId,
                    Text = "First Comment",
                    EntryTypeCode = CommentEntryType.Medication,
                    CreatedDateTime = new DateTime(2020, 1, 1),
                },
                new Comment
                {
                    UserProfileId = this.hdid,
                    ParentEntryId = this.parentEntryId,
                    Text = "Second Comment",
                    EntryTypeCode = CommentEntryType.Medication,
                    CreatedDateTime = new DateTime(2020, 2, 2),
                },
            };
            IMapper autoMapper = MapperUtil.InitializeAutoMapper();
            List<UserComment> userCommentList = commentList.Select(c => CommentMapUtils.CreateFromDbModel(c, cryptoDelegateMock.Object, encryptionKey, autoMapper)).ToList();

            DbResult<IEnumerable<Comment>> commentsDBResult = new()
            {
                Payload = commentList,
                Status = dbResultStatus,
            };

            Mock<ICommentDelegate> commentDelegateMock = new();
            commentDelegateMock.Setup(s => s.GetByParentEntry(this.hdid, this.parentEntryId)).Returns(commentsDBResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                autoMapper);

            RequestResult<IEnumerable<UserComment>> actualResult = service.GetEntryComments(this.hdid, this.parentEntryId);

            return (actualResult, userCommentList);
        }

        private (RequestResult<UserComment> ActualResult, UserComment UserComment) ExecuteInsertComment(DbStatusCode dBStatusCode = DbStatusCode.Created)
        {
            string encryptionKey = "abc";
            DbResult<UserProfile> profileDBResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserComment userComment = new()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Inserted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1),
            };
            IMapper autoMapper = MapperUtil.InitializeAutoMapper();
            Comment comment = CommentMapUtils.ToDbModel(userComment, cryptoDelegateMock.Object, encryptionKey, autoMapper);

            DbResult<Comment> insertResult = new()
            {
                Payload = comment,
                Status = dBStatusCode,
            };

            Mock<ICommentDelegate> commentDelegateMock = new();
            commentDelegateMock.Setup(s => s.Add(It.Is<Comment>(x => x.Text == comment.Text), true)).Returns(insertResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                autoMapper);

            RequestResult<UserComment> actualResult = service.Add(userComment);
            return (actualResult, userComment);
        }
    }
}
