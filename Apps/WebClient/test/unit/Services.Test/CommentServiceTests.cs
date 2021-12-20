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
namespace HealthGateway.WebClient.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
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
            Tuple<RequestResult<IEnumerable<UserComment>>, List<UserComment>> getNotesResult = this.ExecuteGetComments("abc", DBStatusCode.Read);
            var actualResult = getNotesResult.Item1;
            var userCommentList = getNotesResult.Item2;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload?.IsDeepEqual(userCommentList));
        }

        /// <summary>
        /// GetComments - Database error.
        /// </summary>
        [Fact]
        public void ShouldGetCommentsWithDbError()
        {
            Tuple<RequestResult<IEnumerable<UserComment>>, List<UserComment>> getNotesResult = this.ExecuteGetComments("abc", DBStatusCode.Error);
            var actualResult = getNotesResult.Item1;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.True(actualResult?.ResultError?.ErrorCode.EndsWith("-CI-DB", StringComparison.InvariantCulture));
        }

        /// <summary>
        /// InsertComment - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldInsertComment()
        {
            Tuple<RequestResult<UserComment>, UserComment> result = this.ExecuteInsertComment(DBStatusCode.Created);
            var actualResult = result.Item1;
            var createdRecord = result.Item2;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload?.IsDeepEqual(createdRecord));
        }

        /// <summary>
        /// InsertComment - Database Error.
        /// </summary>
        [Fact]
        public void ShouldInsertCommentWithDBError()
        {
            Tuple<RequestResult<UserComment>, UserComment> result = this.ExecuteInsertComment(DBStatusCode.Error);
            var actualResult = result.Item1;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        /// <summary>
        /// UpdateComment - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldUpdateComment()
        {
            Tuple<RequestResult<UserComment>, UserComment> result = this.ExecuteUpdateComment(DBStatusCode.Updated);
            var actualResult = result.Item1;
            var updatedRecord = result.Item2;
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload?.IsDeepEqual(updatedRecord));
        }

        /// <summary>
        /// UpdateComment - Database Error.
        /// </summary>
        [Fact]
        public void ShouldUpdateCommentWithDBError()
        {
            Tuple<RequestResult<UserComment>, UserComment> result = this.ExecuteUpdateComment(DBStatusCode.Error);
            var actualResult = result.Item1;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        /// <summary>
        /// DeleteComment - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldDeleteComment()
        {
            Tuple<RequestResult<UserComment>, UserComment> result = this.ExecuteDeleteComment(DBStatusCode.Deleted);
            var actualResult = result.Item1;
            var deletedRecord = result.Item2;
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload?.IsDeepEqual(deletedRecord));
        }

        /// <summary>
        /// DeleteComment - Database Error.
        /// </summary>
        [Fact]
        public void ShouldDeleteCommentWithDBError()
        {
            Tuple<RequestResult<UserComment>, UserComment> result = this.ExecuteDeleteComment(DBStatusCode.Error);
            var actualResult = result.Item1;

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
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                new Mock<ICommentDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object);

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
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            UserComment userComment = new UserComment()
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
                new Mock<ICryptoDelegate>().Object);

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
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            UserComment userComment = new UserComment()
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
                new Mock<ICryptoDelegate>().Object);

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
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            UserComment userComment = new UserComment()
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
                new Mock<ICryptoDelegate>().Object);

            RequestResult<UserComment> actualResult = service.Delete(userComment);
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        private Tuple<RequestResult<UserComment>, UserComment> ExecuteDeleteComment(DBStatusCode dBStatusCode = DBStatusCode.Deleted)
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserComment userComment = new UserComment()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1),
            };
            Comment comment = userComment.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Comment> deleteResult = new DBResult<Comment>
            {
                Payload = comment,
                Status = dBStatusCode,
            };

            Mock<ICommentDelegate> commentDelegateMock = new Mock<ICommentDelegate>();
            commentDelegateMock.Setup(s => s.Delete(It.Is<Comment>(x => x.Text == comment.Text), true)).Returns(deleteResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object);

            RequestResult<UserComment> actualResult = service.Delete(userComment);
            return new Tuple<RequestResult<UserComment>, UserComment>(actualResult, userComment);
        }

        private Tuple<RequestResult<UserComment>, UserComment> ExecuteUpdateComment(DBStatusCode dBStatusCode = DBStatusCode.Updated)
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserComment userComment = new UserComment()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Updated Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1),
            };

            Comment comment = userComment.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Comment> updateResult = new DBResult<Comment>
            {
                Payload = comment,
                Status = dBStatusCode,
            };

            Mock<ICommentDelegate> commentDelegateMock = new Mock<ICommentDelegate>();
            commentDelegateMock.Setup(s => s.Update(It.Is<Comment>(x => x.Text == comment.Text), true)).Returns(updateResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object);

            RequestResult<UserComment> actualResult = service.Update(userComment);
            return new Tuple<RequestResult<UserComment>, UserComment>(actualResult, userComment);
        }

        private Tuple<RequestResult<IEnumerable<UserComment>>, List<UserComment>> ExecuteGetComments(
            string? encryptionKey = null,
            DBStatusCode dbResultStatus = DBStatusCode.Read)
        {
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            List<Comment> commentList = new List<Comment>();
            commentList.Add(new Comment
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "First Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1),
            });

            commentList.Add(new Comment
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Second Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 2, 2),
            });
            List<UserComment> userCommentList = UserComment.CreateListFromDbModel(commentList, cryptoDelegateMock.Object, encryptionKey).ToList();

            DBResult<IEnumerable<Comment>> commentsDBResult = new DBResult<IEnumerable<Comment>>
            {
                Payload = commentList,
                Status = dbResultStatus,
            };

            Mock<ICommentDelegate> commentDelegateMock = new Mock<ICommentDelegate>();
            commentDelegateMock.Setup(s => s.GetByParentEntry(this.hdid, this.parentEntryId)).Returns(commentsDBResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object);

            RequestResult<IEnumerable<UserComment>> actualResult = service.GetEntryComments(this.hdid, this.parentEntryId);

            return new Tuple<RequestResult<IEnumerable<UserComment>>, List<UserComment>>(actualResult, userCommentList);
        }

        private Tuple<RequestResult<UserComment>, UserComment> ExecuteInsertComment(DBStatusCode dBStatusCode = DBStatusCode.Created)
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey },
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserComment userComment = new UserComment()
            {
                UserProfileId = this.hdid,
                ParentEntryId = this.parentEntryId,
                Text = "Inserted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1),
            };
            Comment comment = userComment.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Comment> insertResult = new DBResult<Comment>
            {
                Payload = comment,
                Status = dBStatusCode,
            };

            Mock<ICommentDelegate> commentDelegateMock = new Mock<ICommentDelegate>();
            commentDelegateMock.Setup(s => s.Add(It.Is<Comment>(x => x.Text == comment.Text), true)).Returns(insertResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object);

            RequestResult<UserComment> actualResult = service.Add(userComment);
            return new Tuple<RequestResult<UserComment>, UserComment>(actualResult, userComment);
        }
    }
}
