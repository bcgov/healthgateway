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
    using Xunit;
    using Moq;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Models;
    using System;
    using System.Linq;
    using HealthGateway.WebClient.Models;
    using System.Collections.Generic;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Database.Constants;
    using HealthGateway.Common.Constants;

    public class CommentServiceTest
    {
        string hdid = "1234567890123456789012345678901234567890123456789012";
        string parentEntryId = "123456789";

        private Tuple<RequestResult<IEnumerable<UserComment>>, List<UserComment>> ExecuteGetComments(string encryptionKey = null, Database.Constants.DBStatusCode dbResultStatus = Database.Constants.DBStatusCode.Read)
        {
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            List<Comment> commentList = new List<Comment>();
            commentList.Add(new Comment
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "First Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1)
            });

            commentList.Add(new Comment
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "Second Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 2, 2)
            });
            List<UserComment> userCommentList = UserComment.CreateListFromDbModel(commentList, cryptoDelegateMock.Object, encryptionKey).ToList();

            DBResult<IEnumerable<Comment>> commentsDBResult = new DBResult<IEnumerable<Comment>>
            {
                Payload = commentList,
                Status = dbResultStatus
            };

            Mock<ICommentDelegate> commentDelegateMock = new Mock<ICommentDelegate>();
            commentDelegateMock.Setup(s => s.GetByParentEntry(hdid, parentEntryId)).Returns(commentsDBResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object
            );

            RequestResult<IEnumerable<UserComment>> actualResult = service.GetEntryComments(hdid, parentEntryId);

            return new Tuple<RequestResult<IEnumerable<UserComment>>, List<UserComment>>(actualResult, userCommentList);
        }

        [Fact]
        public void ShouldGetComments()
        {
            Tuple<RequestResult<IEnumerable<UserComment>>, List<UserComment>> getNotesResult = ExecuteGetComments("abc", Database.Constants.DBStatusCode.Read);
            var actualResult = getNotesResult.Item1;
            var userCommentList = getNotesResult.Item2;

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(userCommentList));
        }

        [Fact]
        public void ShouldGetCommentsWithDbError()
        {
            Tuple<RequestResult<IEnumerable<UserComment>>, List<UserComment>> getNotesResult = ExecuteGetComments("abc", Database.Constants.DBStatusCode.Error);
            var actualResult = getNotesResult.Item1;

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            Assert.Equal("testhostServer-CI-DB", actualResult.ResultError.ErrorCode);
        }

        private Tuple<RequestResult<UserComment>, UserComment> ExecuteInsertComment(Database.Constants.DBStatusCode dBStatusCode = Database.Constants.DBStatusCode.Created)
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserComment userComment = new UserComment()
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "Inserted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1)
            };
            Comment comment = userComment.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Comment> insertResult = new DBResult<Comment>
            {
                Payload = comment,
                Status = dBStatusCode
            };

            Mock<ICommentDelegate> commentDelegateMock = new Mock<ICommentDelegate>();
            commentDelegateMock.Setup(s => s.Add(It.Is<Comment>(x => x.Text == comment.Text), true)).Returns(insertResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object
                );

            RequestResult<UserComment> actualResult = service.Add(userComment);
            return new Tuple<RequestResult<UserComment>, UserComment>(actualResult, userComment);
        }

        [Fact]
        public void ShouldInsertComment()
        {
            Tuple<RequestResult<UserComment>, UserComment> result = ExecuteInsertComment(Database.Constants.DBStatusCode.Created);
            var actualResult = result.Item1;
            var createdRecord = result.Item2;

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(createdRecord));
        }

        [Fact]
        public void ShouldInsertCommentWithDBError()
        {
            Tuple<RequestResult<UserComment>, UserComment> result = ExecuteInsertComment(Database.Constants.DBStatusCode.Error);
            var actualResult = result.Item1;

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        private Tuple<RequestResult<UserComment>, UserComment> ExecuteUpdateComment(Database.Constants.DBStatusCode dBStatusCode = Database.Constants.DBStatusCode.Updated)
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserComment userComment = new UserComment()
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "Updated Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            Comment comment = userComment.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Comment> updateResult = new DBResult<Comment>
            {
                Payload = comment,
                Status = dBStatusCode
            };

            Mock<ICommentDelegate> commentDelegateMock = new Mock<ICommentDelegate>();
            commentDelegateMock.Setup(s => s.Update(It.Is<Comment>(x => x.Text == comment.Text), true)).Returns(updateResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object
            );

            RequestResult<UserComment> actualResult = service.Update(userComment);
            return new Tuple<RequestResult<UserComment>, UserComment>(actualResult, userComment);
        }

        [Fact]
        public void ShouldUpdateComment()
        {
            Tuple<RequestResult<UserComment>, UserComment> result = ExecuteUpdateComment(Database.Constants.DBStatusCode.Updated);
            var actualResult = result.Item1;
            var updatedRecord = result.Item2;
            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(updatedRecord));
        }

        [Fact]
        public void ShouldUpdateCommentWithDBError()
        {
            Tuple<RequestResult<UserComment>, UserComment> result = ExecuteUpdateComment(Database.Constants.DBStatusCode.Error);
            var actualResult = result.Item1;

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        private Tuple<RequestResult<UserComment>, UserComment> ExecuteDeleteComment(Database.Constants.DBStatusCode dBStatusCode = Database.Constants.DBStatusCode.Deleted)
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserComment userComment = new UserComment()
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1)
            };
            Comment comment = userComment.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Comment> deleteResult = new DBResult<Comment>
            {
                Payload = comment,
                Status = dBStatusCode
            };

            Mock<ICommentDelegate> commentDelegateMock = new Mock<ICommentDelegate>();
            commentDelegateMock.Setup(s => s.Delete(It.Is<Comment>(x => x.Text == comment.Text), true)).Returns(deleteResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object
            );

            RequestResult<UserComment> actualResult = service.Delete(userComment);
            return new Tuple<RequestResult<UserComment>, UserComment>(actualResult, userComment);
        }

        [Fact]
        public void ShouldDeleteComment()
        {
            Tuple<RequestResult<UserComment>, UserComment> result = ExecuteDeleteComment(Database.Constants.DBStatusCode.Deleted);
            var actualResult = result.Item1;
            var deletedRecord = result.Item2;
            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(deletedRecord));
        }

        [Fact]
        public void ShouldDeleteCommentWithDBError()
        {
            Tuple<RequestResult<UserComment>, UserComment> result = ExecuteDeleteComment(Database.Constants.DBStatusCode.Error);
            var actualResult = result.Item1;

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        [Fact]
        public void ShouldBeErrorIfNoKeyGet()
        {
            string encryptionKey = null;
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            UserComment userComment = new UserComment()
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = Database.Constants.CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                new Mock<ICommentDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object
            );

            RequestResult<IEnumerable<UserComment>> actualResult = service.GetEntryComments(hdid, parentEntryId);
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        [Fact]
        public void ShouldBeErrorIfNoKeyAdd()
        {
            string encryptionKey = null;
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            UserComment userComment = new UserComment()
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = Database.Constants.CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                new Mock<ICommentDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object
            );

            RequestResult<UserComment> actualResult = service.Add(userComment);
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        [Fact]
        public void ShouldBeErrorIfNoKeyUpdate()
        {
            string encryptionKey = null;
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            UserComment userComment = new UserComment()
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = Database.Constants.CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                new Mock<ICommentDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object
            );


            RequestResult<UserComment> actualResult = service.Update(userComment);
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        [Fact]
        public void ShouldBeErrorIfNoKeyDelete()
        {
            string encryptionKey = null;
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            UserComment userComment = new UserComment()
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = Database.Constants.CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                new Mock<ICommentDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object
            );

            RequestResult<UserComment> actualResult = service.Delete(userComment);
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }
    }
}
