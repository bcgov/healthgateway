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
namespace HealthGateway.WebClient.Test.Controllers
{
    using System;
    using System.Collections.Generic;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.WebClient.Controllers;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    /// <summary>
    /// CommentController's Unit Tests.
    /// </summary>
    public class CommentControllerTests
    {
        private const string Hdid = "mockedHdId";

        /// <summary>
        /// Successfully Create Comment - Happy Path scenario.
        /// </summary>
        [Fact]
        public void ShouldCreateComment()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = new UserComment()
                {
                    Id = Guid.NewGuid(),
                },
                ResultStatus = ResultType.Success,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.Add(expectedResult.ResourcePayload)).Returns(expectedResult);

            CommentController service = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = service.Create(Hdid, expectedResult.ResourcePayload);
            expectedResult.ShouldDeepEqual(actualResult.Value);
        }

        /// <summary>
        /// Create Comment - Bad Request Error scenario.
        /// </summary>
        [Fact]
        public void ShouldCreateCommentWithBadRequestError()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = null, // empty comment
                ResultStatus = ResultType.Success,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.Add(expectedResult.ResourcePayload)).Returns(expectedResult);

            CommentController service = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = service.Create(Hdid, expectedResult.ResourcePayload);

            Assert.IsType<BadRequestResult>(actualResult.Result);
        }

        /// <summary>
        /// Update Comment - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldUpdateComment()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = new UserComment()
                {
                    Id = Guid.NewGuid(),
                    UserProfileId = Hdid,
                },
                ResultStatus = ResultType.Success,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.Update(expectedResult.ResourcePayload)).Returns(expectedResult);

            CommentController service = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = service.Update(Hdid, expectedResult.ResourcePayload);

            RequestResult<UserComment>? actualRequestResult = actualResult?.Value;
            Assert.True(actualRequestResult != null && actualRequestResult.ResultStatus == ResultType.Success);
            Assert.Equal(Hdid, actualRequestResult?.ResourcePayload!.UpdatedBy);
        }

        /// <summary>
        /// Update Comment - ForbidResult Error scenario.
        /// </summary>
        [Fact]
        public void ShouldUpdateCommentWithForbidResultError()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = new UserComment()
                {
                    Id = Guid.NewGuid(),
                },
                ResultStatus = ResultType.Error,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.Update(expectedResult.ResourcePayload)).Returns(expectedResult);

            CommentController service = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = service.Update(Hdid, expectedResult.ResourcePayload);

            Assert.IsType<ForbidResult>(actualResult.Result);
        }

        /// <summary>
        /// Update Comment - BadRequest Error scenario.
        /// </summary>
        [Fact]
        public void ShouldUpdateCommentWithBadRequestError()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = null,
                ResultStatus = ResultType.Error,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.Update(expectedResult.ResourcePayload)).Returns(expectedResult);

            CommentController service = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = service.Update(Hdid, expectedResult.ResourcePayload);

            Assert.IsType<BadRequestResult>(actualResult.Result);
        }

        /// <summary>
        /// Delete Comment - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldDeleteComment()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = new UserComment()
                {
                    Id = Guid.NewGuid(),
                    UserProfileId = Hdid,
                },
                ResultStatus = ResultType.Success,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.Delete(expectedResult.ResourcePayload)).Returns(expectedResult);

            CommentController service = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = service.Delete(Hdid, expectedResult.ResourcePayload);

            RequestResult<UserComment>? actualRequestResult = actualResult?.Value;
            Assert.NotNull(actualRequestResult);
            Assert.Equal(ResultType.Success, actualRequestResult?.ResultStatus);
            expectedResult.ShouldDeepEqual(actualRequestResult);
        }

        /// <summary>
        /// Delete Comment - ForbidResult Error scenario.
        /// </summary>
        [Fact]
        public void ShouldDeleteCommentWithForbidResultError()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = new UserComment()
                {
                    Id = Guid.NewGuid(),
                },
                ResultStatus = ResultType.Error,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.Delete(expectedResult.ResourcePayload)).Returns(expectedResult);

            CommentController service = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = service.Delete(Hdid, expectedResult.ResourcePayload);

            Assert.IsType<ForbidResult>(actualResult.Result);
        }

        /// <summary>
        /// GetAllForEntry - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldGetAllForEntry()
        {
            List<UserComment> mockedComments = new();
            for (int i = 0; i < 10; i++)
            {
                mockedComments.Add(new UserComment()
                {
                    Text = "comment " + i,
                    UserProfileId = Hdid,
                });
            }

            RequestResult<IEnumerable<UserComment>> expectedResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = mockedComments,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.GetEntryComments(It.IsAny<string>(), It.IsAny<string>())).Returns(expectedResult);

            CommentController service = new(commentServiceMock.Object);

            RequestResult<IEnumerable<UserComment>> actualResult = service.GetAllForEntry(Hdid, "parentEntryIdMock");

            Assert.NotNull(actualResult);
            Assert.Equal(ResultType.Success, actualResult?.ResultStatus);
        }
    }
}
