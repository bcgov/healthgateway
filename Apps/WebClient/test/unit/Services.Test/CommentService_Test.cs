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
    using HealthGateway.WebClient.Models;
    using System.Collections.Generic;

    public class CommentServiceTest
    {
        string hdid = "1234567890123456789012345678901234567890123456789012";
        string parentEntryId = "123456789";

        [Fact]
        public void ShouldGetComments()
        {
            List<Comment> commentList = new List<Comment>();
            commentList.Add(new Comment
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "First Comment",
                EntryTypeCode = Database.Constant.CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1)
            });

            commentList.Add(new Comment
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "Second Comment",
                EntryTypeCode = Database.Constant.CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 2, 2)
            });

            DBResult<IEnumerable<Comment>> commentsDBResult = new DBResult<IEnumerable<Comment>>
            {
                Payload = commentList,
                Status = Database.Constant.DBStatusCode.Read
            };

            Mock<ICommentDelegate> commentDelegateMock = new Mock<ICommentDelegate>();
            commentDelegateMock.Setup(s => s.GetList(hdid, parentEntryId)).Returns(commentsDBResult);

            Mock<IConfigurationService> configServiceMock = new Mock<IConfigurationService>();
            configServiceMock.Setup(s => s.GetConfiguration()).Returns(new ExternalConfiguration());

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object
            );
            RequestResult<IEnumerable<Comment>> actualResult = service.GetList(hdid, parentEntryId);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(commentList));
        }

        [Fact]
        public void ShouldInsertComment()
        {
            Comment comment = new Comment()
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "Inserted Comment",
                EntryTypeCode = Database.Constant.CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            DBResult<Comment> insertResult = new DBResult<Comment>
            {
                Payload = comment,
                Status = Database.Constant.DBStatusCode.Created               
            };

            Mock<ICommentDelegate> commentDelegateMock = new Mock<ICommentDelegate>();
            commentDelegateMock.Setup(s => s.Add(comment, true)).Returns(insertResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object);


            RequestResult<Comment> actualResult = service.Add(comment);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(comment));
        }

        [Fact]
        public void ShouldUpdateComment()
        {
            Comment comment = new Comment()
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "Updated Comment",
                EntryTypeCode = Database.Constant.CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            DBResult<Comment> updateResult = new DBResult<Comment>
            {
                Payload = comment,
                Status = Database.Constant.DBStatusCode.Updated
            };

            Mock<ICommentDelegate> commentDelegateMock = new Mock<ICommentDelegate>();
            commentDelegateMock.Setup(s => s.Update(comment, true)).Returns(updateResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object);


            RequestResult<Comment> actualResult = service.Update(comment);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(comment));
        }

        [Fact]
        public void ShouldDeleteComment()
        {
            Comment comment = new Comment()
            {
                UserProfileId = hdid,
                ParentEntryId = parentEntryId,
                Text = "Deleted Comment",
                EntryTypeCode = Database.Constant.CommentEntryType.Medication,
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            DBResult<Comment> deleteResult = new DBResult<Comment>
            {
                Payload = comment,
                Status = Database.Constant.DBStatusCode.Deleted
            };

            Mock<ICommentDelegate> commentDelegateMock = new Mock<ICommentDelegate>();
            commentDelegateMock.Setup(s => s.Delete(comment, true)).Returns(deleteResult);

            ICommentService service = new CommentService(
                new Mock<ILogger<CommentService>>().Object,
                commentDelegateMock.Object);


            RequestResult<Comment> actualResult = service.Delete(comment);

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(comment));
        }
    }
}
