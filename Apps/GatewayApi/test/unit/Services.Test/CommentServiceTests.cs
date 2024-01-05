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
        private const string EncryptionKey = "abc";
        private const string Hdid = "1234567890123456789012345678901234567890123456789012";
        private const string ParentEntryId = "123456789";

        /// <summary>
        /// GetEntryComments.
        /// </summary>
        /// <param name="dbStatusCode">The status code for the DbResult.</param>
        /// <param name="encryptionKey">The encryption key used to encrypt and decrypt.</param>
        /// <param name="success">The bool value indicating whether test case is successful or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Read, EncryptionKey, true)]
        [InlineData(DbStatusCode.Error, EncryptionKey, false)]
        [InlineData(null, null, false)]
        public async Task ShouldGetEntryComments(DbStatusCode? dbStatusCode, string? encryptionKey, bool success)
        {
            // Arrange
            List<Comment> comments =
            [
                new Comment
                {
                    UserProfileId = Hdid,
                    ParentEntryId = ParentEntryId,
                    Text = "First Comment",
                    EntryTypeCode = CommentEntryType.Medication,
                    CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
                },

                new Comment
                {
                    UserProfileId = Hdid,
                    ParentEntryId = ParentEntryId,
                    Text = "Second Comment",
                    EntryTypeCode = CommentEntryType.Medication,
                    CreatedDateTime = DateTime.Parse("2020-02-02", CultureInfo.InvariantCulture),
                },
            ];

            DbResult<IEnumerable<Comment>>? dbResult = dbStatusCode != null
                ? new()
                {
                    Payload = comments,
                    Status = dbStatusCode.Value,
                }
                : null;

            Mock<ICryptoDelegate> cryptoDelegateMock = GetCryptoDelegateMock();
            List<UserComment> expected = comments.Select(c => CommentMapUtils.CreateFromDbModel(c, cryptoDelegateMock.Object, EncryptionKey, MapperUtil.InitializeAutoMapper())).ToList();

            CommentService service = GetCommentService(encryptionKey, commentsDbResult: dbResult);

            // Act
            RequestResult<IEnumerable<UserComment>> actual = await service.GetEntryCommentsAsync(Hdid, ParentEntryId);

            // Assert
            if (success)
            {
                Assert.Equal(ResultType.Success, actual.ResultStatus);
                expected.ShouldDeepEqual(actual.ResourcePayload);
            }
            else
            {
                Assert.Equal(ResultType.Error, actual.ResultStatus);
                Assert.NotNull(actual.ResultError);
            }
        }

        /// <summary>
        /// GetProfileComments.
        /// </summary>
        /// <param name="dbStatusCode">The status code for the DbResult.</param>
        /// <param name="encryptionKey">The encryption key used to encrypt and decrypt.</param>
        /// <param name="success">The bool value indicating whether test case is successful or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Read, EncryptionKey, true)]
        [InlineData(DbStatusCode.Error, EncryptionKey, false)]
        [InlineData(null, null, false)]
        public async Task ShouldGetProfileComments(DbStatusCode? dbStatusCode, string? encryptionKey, bool success)
        {
            // Arrange
            List<Comment> comments =
            [
                new Comment
                {
                    UserProfileId = Hdid,
                    ParentEntryId = ParentEntryId,
                    Text = "First Comment",
                    EntryTypeCode = CommentEntryType.Medication,
                    CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
                },
            ];

            DbResult<IEnumerable<Comment>>? dbResult = dbStatusCode != null
                ? new()
                {
                    Payload = comments,
                    Status = dbStatusCode.Value,
                }
                : null;

            Mock<ICryptoDelegate> cryptoDelegateMock = GetCryptoDelegateMock();
            IEnumerable<UserComment> userComments = comments.Select(c => CommentMapUtils.CreateFromDbModel(c, cryptoDelegateMock.Object, EncryptionKey, MapperUtil.InitializeAutoMapper()));
            IDictionary<string, IEnumerable<UserComment>> expected = userComments.GroupBy(x => x.ParentEntryId).ToDictionary(g => g.Key, g => g.AsEnumerable());

            CommentService service = GetCommentService(encryptionKey, commentsDbResult: dbResult);

            // Act
            RequestResult<IDictionary<string, IEnumerable<UserComment>>> actual = await service.GetProfileCommentsAsync(Hdid);

            // Assert
            if (success)
            {
                Assert.Equal(ResultType.Success, actual.ResultStatus);
                expected.ShouldDeepEqual(actual.ResourcePayload);
            }
            else
            {
                Assert.Equal(ResultType.Error, actual.ResultStatus);
                Assert.NotNull(actual.ResultError);
            }
        }

        /// <summary>
        /// InsertComment.
        /// </summary>
        /// <param name="dbStatusCode">The status code for the DbResult.</param>
        /// <param name="encryptionKey">The encryption key used to encrypt and decrypt.</param>
        /// <param name="hdid">The hdid associated with the comment.</param>
        /// <param name="text">The text associated with the comment.</param>
        /// <param name="success">The bool value indicating whether test case is successful or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Created, EncryptionKey, Hdid, "Inserted Comment", true)]
        [InlineData(DbStatusCode.Error, EncryptionKey, Hdid, "Inserted Comment", false)]
        [InlineData(null, null, Hdid, null, false)]
        [InlineData(null, null, null, "Inserted Comment", false)]
        public async Task ShouldInsertComment(DbStatusCode? dbStatusCode, string? encryptionKey, string? hdid, string? text, bool success)
        {
            // Arrange
            UserComment expected = new()
            {
                UserProfileId = hdid!,
                ParentEntryId = ParentEntryId,
                Text = text!,
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            Mock<ICryptoDelegate> cryptoDelegateMock = GetCryptoDelegateMock();
            Comment comment = CommentMapUtils.ToDbModel(expected, cryptoDelegateMock.Object, EncryptionKey, MapperUtil.InitializeAutoMapper());

            DbResult<Comment>? dbResult = dbStatusCode != null
                ? new()
                {
                    Payload = comment,
                    Status = dbStatusCode.Value,
                }
                : null;

            CommentService service = GetCommentService(encryptionKey, dbResult);

            // Act
            RequestResult<UserComment> actual = await service.AddAsync(expected);

            // Assert
            if (success)
            {
                Assert.Equal(ResultType.Success, actual.ResultStatus);
                expected.ShouldDeepEqual(actual.ResourcePayload);
            }
            else
            {
                Assert.Equal(ResultType.Error, actual.ResultStatus);
                Assert.NotNull(actual.ResultError);
            }
        }

        /// <summary>
        /// UpdateComment.
        /// </summary>
        /// <param name="dbStatusCode">The status code for the DbResult.</param>
        /// <param name="encryptionKey">The encryption key used to encrypt and decrypt.</param>
        /// <param name="hdid">The hdid associated with the comment.</param>
        /// <param name="text">The text associated with the comment.</param>
        /// <param name="success">The bool value indicating whether test case is successful or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Updated, EncryptionKey, Hdid, "Updated Comment", true)]
        [InlineData(DbStatusCode.Error, EncryptionKey, Hdid, "Updated Comment", false)]
        [InlineData(null, null, Hdid, null, false)]
        [InlineData(null, null, null, "Updated Comment", false)]
        public async Task ShouldUpdateComment(DbStatusCode? dbStatusCode, string? encryptionKey, string? hdid, string? text, bool success)
        {
            // Arrange
            UserComment expected = new()
            {
                UserProfileId = hdid!,
                ParentEntryId = ParentEntryId,
                Text = text!,
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            Mock<ICryptoDelegate> cryptoDelegateMock = GetCryptoDelegateMock();
            Comment comment = CommentMapUtils.ToDbModel(expected, cryptoDelegateMock.Object, EncryptionKey, MapperUtil.InitializeAutoMapper());

            DbResult<Comment>? dbResult = dbStatusCode != null
                ? new()
                {
                    Payload = comment,
                    Status = dbStatusCode.Value,
                }
                : null;

            CommentService service = GetCommentService(encryptionKey, dbResult);

            // Act
            RequestResult<UserComment> actual = await service.UpdateAsync(expected);

            // Assert
            if (success)
            {
                Assert.Equal(ResultType.Success, actual.ResultStatus);
                expected.ShouldDeepEqual(actual.ResourcePayload);
            }
            else
            {
                Assert.Equal(ResultType.Error, actual.ResultStatus);
                Assert.NotNull(actual.ResultError);
            }
        }

        /// <summary>
        /// DeleteComment.
        /// </summary>
        /// <param name="dbStatusCode">The status code for the DbResult.</param>
        /// <param name="encryptionKey">The encryption key used to encrypt and decrypt.</param>
        /// <param name="hdid">The hdid associated with the comment.</param>
        /// <param name="text">The text associated with the comment.</param>
        /// <param name="success">The bool value indicating whether test case is successful or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Deleted, EncryptionKey, Hdid, "Deleted Comment", true)]
        [InlineData(DbStatusCode.Error, EncryptionKey, Hdid, "Deleted Comment", false)]
        [InlineData(null, null, Hdid, null, false)]
        [InlineData(null, null, null, "Deleted Comment", false)]
        public async Task ShouldDeleteComment(DbStatusCode? dbStatusCode, string? encryptionKey, string? hdid, string? text, bool success)
        {
            // Arrange
            UserComment expected = new()
            {
                UserProfileId = hdid!,
                ParentEntryId = ParentEntryId,
                Text = text!,
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture),
            };

            Mock<ICryptoDelegate> cryptoDelegateMock = GetCryptoDelegateMock();
            Comment comment = CommentMapUtils.ToDbModel(expected, cryptoDelegateMock.Object, EncryptionKey, MapperUtil.InitializeAutoMapper());

            DbResult<Comment>? dbResult = dbStatusCode != null
                ? new()
                {
                    Payload = comment,
                    Status = dbStatusCode.Value,
                }
                : null;

            CommentService service = GetCommentService(encryptionKey, dbResult);

            // Act
            RequestResult<UserComment> actual = await service.DeleteAsync(expected);

            // Assert
            if (success)
            {
                Assert.Equal(ResultType.Success, actual.ResultStatus);
                expected.ShouldDeepEqual(actual.ResourcePayload);
            }
            else
            {
                Assert.Equal(ResultType.Error, actual.ResultStatus);
                Assert.NotNull(actual.ResultError);
            }
        }

        private static Mock<ICryptoDelegate> GetCryptoDelegateMock()
        {
            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));
            return cryptoDelegateMock;
        }

        private static CommentService GetCommentService(string? encryptionKey, DbResult<Comment>? commentDbResult = null, DbResult<IEnumerable<Comment>>? commentsDbResult = null)
        {
            UserProfile userProfile = new()
                { EncryptionKey = encryptionKey };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock.Setup(s => s.GetUserProfileAsync(Hdid, It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);

            Mock<ICommentDelegate> commentDelegateMock = new();
            IMapper autoMapper = MapperUtil.InitializeAutoMapper();

            if (commentDbResult != null)
            {
                commentDelegateMock.Setup(s => s.AddAsync(It.Is<Comment>(x => x.Text == commentDbResult.Payload.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(commentDbResult);
                commentDelegateMock.Setup(s => s.UpdateAsync(It.Is<Comment>(x => x.Text == commentDbResult.Payload.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(commentDbResult);
                commentDelegateMock.Setup(s => s.DeleteAsync(It.Is<Comment>(x => x.Text == commentDbResult.Payload.Text), true, It.IsAny<CancellationToken>())).ReturnsAsync(commentDbResult);
            }

            if (commentsDbResult != null)
            {
                commentDelegateMock.Setup(s => s.GetByParentEntryAsync(Hdid, ParentEntryId, It.IsAny<CancellationToken>())).ReturnsAsync(commentsDbResult);
                commentDelegateMock.Setup(s => s.GetAllAsync(Hdid, It.IsAny<CancellationToken>())).ReturnsAsync(commentsDbResult);
            }

            return new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                GetCryptoDelegateMock().Object,
                autoMapper);
        }
    }
}
