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
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using Hangfire;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserFeedbackService's Unit Tests.
    /// </summary>
    public class UserFeedbackServiceTests
    {
        /// <summary>
        /// CreateRating - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateRating()
        {
            Rating expectedRating = new()
            {
                RatingValue = 5,
                Skip = false,
            };

            DbResult<Rating> insertResult = new()
            {
                Payload = expectedRating,
                Status = DbStatusCode.Created,
            };

            Mock<IRatingDelegate> ratingDelegateMock = new();
            ratingDelegateMock.Setup(s => s.InsertRatingAsync(It.Is<Rating>(r => r.RatingValue == expectedRating.RatingValue && r.Skip == expectedRating.Skip), It.IsAny<CancellationToken>()))
                .ReturnsAsync(insertResult);

            Mock<IBackgroundJobClient> mockJobclient = new();
            Mock<IUserProfileDelegate> mockProfileDelegate = new();

            IUserFeedbackService service = new UserFeedbackService(
                new Mock<ILogger<UserFeedbackService>>().Object,
                new Mock<IFeedbackDelegate>().Object,
                ratingDelegateMock.Object,
                mockProfileDelegate.Object,
                mockJobclient.Object);

            RequestResult<Rating> actualResult = await service.CreateRatingAsync(expectedRating);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            expectedRating.ShouldDeepEqual(actualResult.ResourcePayload);
        }

        /// <summary>
        /// CreateRating - Database Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateRatingWithError()
        {
            Rating expectedRating = new()
            {
                RatingValue = 5,
                Skip = false,
            };

            DbResult<Rating> insertResult = new()
            {
                Payload = expectedRating,
                Status = DbStatusCode.Error,
            };

            Mock<IRatingDelegate> ratingDelegateMock = new();
            ratingDelegateMock.Setup(s => s.InsertRatingAsync(It.Is<Rating>(r => r.RatingValue == expectedRating.RatingValue && r.Skip == expectedRating.Skip), It.IsAny<CancellationToken>()))
                .ReturnsAsync(insertResult);

            Mock<IBackgroundJobClient> mockJobclient = new();
            Mock<IUserProfileDelegate> mockProfileDelegate = new();

            IUserFeedbackService service = new UserFeedbackService(
                new Mock<ILogger<UserFeedbackService>>().Object,
                new Mock<IFeedbackDelegate>().Object,
                ratingDelegateMock.Object,
                mockProfileDelegate.Object,
                mockJobclient.Object);

            RequestResult<Rating> actualResult = await service.CreateRatingAsync(expectedRating);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// CreateUserFeedBack - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserFeedback()
        {
            UserFeedback expectedUserFeedback = new()
            {
                Comment = "Mocked Comment",
                Id = Guid.NewGuid(),
                UserProfileId = "Mocked UserProfileId",
                IsSatisfied = true,
                IsReviewed = true,
            };

            DbResult<UserFeedback> insertResult = new()
            {
                Payload = expectedUserFeedback,
                Status = DbStatusCode.Created,
            };

            UserProfile profile = new()
            {
                Email = "mock@email.com",
            };

            Mock<IFeedbackDelegate> userFeedbackDelegateMock = new();
            userFeedbackDelegateMock.Setup(
                    s => s.InsertUserFeedbackAsync(
                        It.Is<UserFeedback>(
                            r => r.Comment == expectedUserFeedback.Comment && r.Id == expectedUserFeedback.Id && r.UserProfileId == expectedUserFeedback.UserProfileId &&
                                 r.IsSatisfied == expectedUserFeedback.IsSatisfied && r.IsReviewed == expectedUserFeedback.IsReviewed),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(insertResult);

            Mock<IBackgroundJobClient> mockJobclient = new();
            Mock<IUserProfileDelegate> mockProfileDelegate = new();
            mockProfileDelegate.Setup(s => s.GetUserProfileAsync(It.Is<string>(hdid => hdid == expectedUserFeedback.UserProfileId), It.IsAny<CancellationToken>())).ReturnsAsync(profile);

            IUserFeedbackService service = new UserFeedbackService(
                new Mock<ILogger<UserFeedbackService>>().Object,
                userFeedbackDelegateMock.Object,
                new Mock<IRatingDelegate>().Object,
                mockProfileDelegate.Object,
                mockJobclient.Object);

            DbResult<UserFeedback> actualResult = await service.CreateUserFeedbackAsync(expectedUserFeedback);

            Assert.Equal(DbStatusCode.Created, actualResult.Status);
            expectedUserFeedback.ShouldDeepEqual(actualResult.Payload);
        }
    }
}
