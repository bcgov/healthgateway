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
namespace HealthGateway.GatewayApiTests.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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
    using HealthGateway.GatewayApiTests.Services.Test.Utils;
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
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetComments()
        {
            (Task<RequestResult<IEnumerable<UserComment>>> task, List<UserComment> userCommentList) = this.ExecuteGetComments("abc");
            RequestResult<IEnumerable<UserComment>> actualResult = await task;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            userCommentList.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetComments - Database error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetCommentsWithDbError()
        {
            (Task<RequestResult<IEnumerable<UserComment>>> task, _) = this.ExecuteGetComments("abc", DbStatusCode.Error);
            RequestResult<IEnumerable<UserComment>> actualResult = await task;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.True(actualResult.ResultError?.ErrorCode.EndsWith("-CI-DB", StringComparison.InvariantCulture));
        }

        /// <summary>
        /// InsertComment - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldInsertComment()
        {
            (Task<RequestResult<UserComment>> task, UserComment createdRecord) = this.ExecuteInsertComment();
            RequestResult<UserComment> actualResult = await task;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            createdRecord.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// InsertComment - Database Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldInsertCommentWithDbError()
        {
            (Task<RequestResult<UserComment>> task, _) = this.ExecuteInsertComment(DbStatusCode.Error);
            RequestResult<UserComment> actualResult = await task;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        /// <summary>
        /// UpdateComment - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateComment()
        {
            (Task<RequestResult<UserComment>> task, UserComment updatedRecord) = this.ExecuteUpdateComment();
            RequestResult<UserComment> actualResult = await task;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            updatedRecord.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// UpdateComment - Database Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateCommentWithDbError()
        {
            (Task<RequestResult<UserComment>> task, _) = this.ExecuteUpdateComment(DbStatusCode.Error);
            RequestResult<UserComment> actualResult = await task;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        /// <summary>
        /// DeleteComment - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDeleteComment()
        {
            (Task<RequestResult<UserComment>> task, UserComment deletedRecord) = this.ExecuteDeleteComment();
            RequestResult<UserComment> actualResult = await task;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            deletedRecord.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// DeleteComment - Database Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDeleteCommentWithDbError()
        {
            (Task<RequestResult<UserComment>> task, _) = this.ExecuteDeleteComment(DbStatusCode.Error);
            RequestResult<UserComment> actualResult = await task;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        /// <summary>
        /// GetComments - No Encryption key error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetCommentsWithNoKeyError()
        {
            string? encryptionKey = null;
            DbResult<UserProfile> profileDbResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDbResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                new Mock<ICommentDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                MapperUtil.InitializeAutoMapper());

            RequestResult<IEnumerable<UserComment>> actualResult = await service.GetEntryCommentsAsync(this.hdid, this.parentEntryId);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// InsertComment - No Encryption key error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldInsertCommentWithNoKeyError()
        {
            string? encryptionKey = null;
            DbResult<UserProfile> profileDbResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDbResult);

            UserComment userComment = new()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                new Mock<ICommentDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                MapperUtil.InitializeAutoMapper());

            RequestResult<UserComment> actualResult = await service.AddAsync(userComment);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// UpdateComment - No Encryption key error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateCommentWithNoKeyError()
        {
            string? encryptionKey = null;
            DbResult<UserProfile> profileDbResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDbResult);

            UserComment userComment = new()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                new Mock<ICommentDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                MapperUtil.InitializeAutoMapper());

            RequestResult<UserComment> actualResult = await service.UpdateAsync(userComment);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// DeleteComment - No Encryption key error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDeleteCommentWithNoKeyError()
        {
            string? encryptionKey = null;
            DbResult<UserProfile> profileDbResult = new()
            {
                Payload = new UserProfile
                    { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDbResult);

            UserComment userComment = new()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                new Mock<ICommentDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                MapperUtil.InitializeAutoMapper());

            RequestResult<UserComment> actualResult = await service.DeleteAsync(userComment);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        private (Task<RequestResult<UserComment>> ActualResult, UserComment UserComment) ExecuteDeleteComment(DbStatusCode dBStatusCode = DbStatusCode.Deleted)
        {
            string encryptionKey = "abc";
            UserProfile userProfile = new()
                { EncryptionKey = encryptionKey };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfileAsync(this.hdid, It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserComment userComment = new()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };
            IMapper autoMapper = MapperUtil.InitializeAutoMapper();
            Comment comment = CommentMapUtils.ToDbModel(userComment, cryptoDelegateMock.Object, encryptionKey, autoMapper);

            DbResult<Comment> deleteResult = new()
            {
                Payload = comment,
                Status = dBStatusCode,
            };

            Mock<ICommentDelegate> commentDelegateMock = new();
            commentDelegateMock.Setup(s => s.DeleteAsync(It.Is<Comment>(x => x.Text == comment.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(deleteResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                autoMapper);

            Task<RequestResult<UserComment>> actualResult = service.DeleteAsync(userComment);
            return (actualResult, userComment);
        }

        private (Task<RequestResult<UserComment>> ActualResult, UserComment UserComment) ExecuteUpdateComment(DbStatusCode dBStatusCode = DbStatusCode.Updated)
        {
            string encryptionKey = "abc";
            UserProfile userProfile = new()
                { EncryptionKey = encryptionKey };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfileAsync(this.hdid, It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserComment userComment = new()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Updated Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            IMapper autoMapper = MapperUtil.InitializeAutoMapper();
            Comment comment = CommentMapUtils.ToDbModel(userComment, cryptoDelegateMock.Object, encryptionKey, autoMapper);

            DbResult<Comment> updateResult = new()
            {
                Payload = comment,
                Status = dBStatusCode,
            };

            Mock<ICommentDelegate> commentDelegateMock = new();
            commentDelegateMock.Setup(s => s.UpdateAsync(It.Is<Comment>(x => x.Text == comment.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(updateResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                autoMapper);

            Task<RequestResult<UserComment>> actualResult = service.UpdateAsync(userComment);
            return (actualResult, userComment);
        }

        private (Task<RequestResult<IEnumerable<UserComment>>> ActualResult, List<UserComment> UserCommentList) ExecuteGetComments(
            string? encryptionKey = null,
            DbStatusCode dbResultStatus = DbStatusCode.Read)
        {
            UserProfile userProfile = new()
                { EncryptionKey = encryptionKey };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfileAsync(this.hdid, It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);

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
                    CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
                },
                new Comment
                {
                    UserProfileId = this.hdid,
                    ParentEntryId = this.parentEntryId,
                    Text = "Second Comment",
                    EntryTypeCode = CommentEntryType.Medication,
                    CreatedDateTime = DateTime.Parse("2020-02-02", CultureInfo.InvariantCulture),
                },
            };
            IMapper autoMapper = MapperUtil.InitializeAutoMapper();
            List<UserComment> userCommentList = commentList.Select(c => CommentMapUtils.CreateFromDbModel(c, cryptoDelegateMock.Object, encryptionKey, autoMapper)).ToList();

            DbResult<IList<Comment>> commentsDbResult = new()
            {
                Payload = commentList,
                Status = dbResultStatus,
            };

            Mock<ICommentDelegate> commentDelegateMock = new();
            commentDelegateMock.Setup(s => s.GetByParentEntryAsync(this.hdid, this.parentEntryId, It.IsAny<CancellationToken>())).ReturnsAsync(commentsDbResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                autoMapper);

            Task<RequestResult<IEnumerable<UserComment>>> actualResult = service.GetEntryCommentsAsync(this.hdid, this.parentEntryId);

            return (actualResult, userCommentList);
        }

        private (Task<RequestResult<UserComment>> ActualResult, UserComment UserComment) ExecuteInsertComment(DbStatusCode dBStatusCode = DbStatusCode.Created)
        {
            string encryptionKey = "abc";
            UserProfile userProfile = new()
                { EncryptionKey = encryptionKey };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfileAsync(this.hdid, It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);

            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserComment userComment = new()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Inserted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };
            IMapper autoMapper = MapperUtil.InitializeAutoMapper();
            Comment comment = CommentMapUtils.ToDbModel(userComment, cryptoDelegateMock.Object, encryptionKey, autoMapper);

            DbResult<Comment> insertResult = new()
            {
                Payload = comment,
                Status = dBStatusCode,
            };

            Mock<ICommentDelegate> commentDelegateMock = new();
            commentDelegateMock.Setup(s => s.AddAsync(It.Is<Comment>(x => x.Text == comment.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(insertResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object,
                autoMapper);

            Task<RequestResult<UserComment>> actualResult = service.AddAsync(userComment);
            return (actualResult, userComment);
        }
    }
}
