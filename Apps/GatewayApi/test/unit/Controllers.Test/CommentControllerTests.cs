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
namespace HealthGateway.GatewayApiTests.Controllers.Test
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.GatewayApi.Controllers;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
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
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateComment()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = new UserComment
                {
                    Id = Guid.NewGuid(),
                },
                ResultStatus = ResultType.Success,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.AddAsync(expectedResult.ResourcePayload, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(expectedResult));

            CommentController controller = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = await controller.Create(Hdid, expectedResult.ResourcePayload, It.IsAny<CancellationToken>());
            expectedResult.ShouldDeepEqual(actualResult.Value);
        }

        /// <summary>
        /// Create Comment - Bad Request Error scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateCommentWithBadRequestError()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = null, // empty comment
                ResultStatus = ResultType.Success,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.AddAsync(expectedResult.ResourcePayload, It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResult));

            CommentController controller = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = await controller.Create(Hdid, expectedResult.ResourcePayload, It.IsAny<CancellationToken>());

            Assert.IsType<BadRequestResult>(actualResult.Result);
        }

        /// <summary>
        /// Update Comment - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateComment()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = new UserComment
                {
                    Id = Guid.NewGuid(),
                    UserProfileId = Hdid,
                },
                ResultStatus = ResultType.Success,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.UpdateAsync(expectedResult.ResourcePayload, It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResult));

            CommentController controller = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = await controller.Update(Hdid, expectedResult.ResourcePayload, It.IsAny<CancellationToken>());

            RequestResult<UserComment>? actualRequestResult = actualResult.Value;
            Assert.True(actualRequestResult != null && actualRequestResult.ResultStatus == ResultType.Success);
            Assert.Equal(Hdid, actualRequestResult.ResourcePayload!.UpdatedBy);
        }

        /// <summary>
        /// Update Comment - ForbidResult Error scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateCommentWithForbidResultError()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = new UserComment
                {
                    Id = Guid.NewGuid(),
                },
                ResultStatus = ResultType.Error,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.UpdateAsync(expectedResult.ResourcePayload, It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResult));

            CommentController service = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = await service.Update(Hdid, expectedResult.ResourcePayload, It.IsAny<CancellationToken>());

            Assert.IsType<ForbidResult>(actualResult.Result);
        }

        /// <summary>
        /// Update Comment - BadRequest Error scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateCommentWithBadRequestError()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = null,
                ResultStatus = ResultType.Error,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.UpdateAsync(expectedResult.ResourcePayload, It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResult));

            CommentController service = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = await service.Update(Hdid, expectedResult.ResourcePayload, It.IsAny<CancellationToken>());

            Assert.IsType<BadRequestResult>(actualResult.Result);
        }

        /// <summary>
        /// Delete Comment - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDeleteComment()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = new UserComment
                {
                    Id = Guid.NewGuid(),
                    UserProfileId = Hdid,
                },
                ResultStatus = ResultType.Success,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.DeleteAsync(expectedResult.ResourcePayload, It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResult));

            CommentController service = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = await service.Delete(Hdid, expectedResult.ResourcePayload, It.IsAny<CancellationToken>());

            RequestResult<UserComment>? actualRequestResult = actualResult.Value;
            Assert.NotNull(actualRequestResult);
            Assert.Equal(ResultType.Success, actualRequestResult.ResultStatus);
            expectedResult.ShouldDeepEqual(actualRequestResult);
        }

        /// <summary>
        /// Delete Comment - ForbidResult Error scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDeleteCommentWithForbidResultError()
        {
            RequestResult<UserComment> expectedResult = new()
            {
                ResourcePayload = new UserComment
                {
                    Id = Guid.NewGuid(),
                },
                ResultStatus = ResultType.Error,
            };

            Mock<ICommentService> commentServiceMock = new();
            commentServiceMock.Setup(s => s.DeleteAsync(expectedResult.ResourcePayload, It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResult));

            CommentController service = new(commentServiceMock.Object);

            ActionResult<RequestResult<UserComment>> actualResult = await service.Delete(Hdid, expectedResult.ResourcePayload, It.IsAny<CancellationToken>());

            Assert.IsType<ForbidResult>(actualResult.Result);
        }

        /// <summary>
        /// GetAllForEntryAsync - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetAllForEntry()
        {
            List<UserComment> mockedComments = new();
            for (int i = 0; i < 10; i++)
            {
                mockedComments.Add(
                    new UserComment
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
            commentServiceMock.Setup(s => s.GetEntryCommentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResult));

            CommentController controller = new(commentServiceMock.Object);

            RequestResult<IEnumerable<UserComment>> actualResult = await controller.GetAllForEntry(Hdid, "parentEntryIdMock", It.IsAny<CancellationToken>());

            Assert.NotNull(actualResult);
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
        }
    }
}
