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
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Controllers;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserFeedbackController's Unit Tests.
    /// </summary>
    public class UserFeedbackControllerTests
    {
        private const string Hdid = "mockedHdId";

        /// <summary>
        /// CreateUserFeedback - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserFeedback()
        {
            Feedback feedback = new();
            UserFeedback userFeedback = new()
            {
                UserProfileId = Hdid,
                CreatedBy = Hdid,
                UpdatedBy = Hdid,
            };

            DbResult<UserFeedback> mockedDbResult = new()
            {
                Status = DbStatusCode.Created,
                Payload = userFeedback,
            };

            Mock<IUserFeedbackService> userFeedbackServiceMock = new();
            userFeedbackServiceMock.Setup(s => s.CreateUserFeedbackAsync(It.IsAny<Feedback>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockedDbResult);

            UserFeedbackController controller = new(userFeedbackServiceMock.Object);
            IActionResult actualResult = await controller.CreateUserFeedback(Hdid, feedback, default);

            Assert.IsType<OkResult>(actualResult);
        }

        /// <summary>
        /// CreateUserFeedback - ConflictResult Error scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserFeedbackWithConflictResultError()
        {
            Feedback feedback = new();
            UserFeedback userFeedback = new()
            {
                UserProfileId = Hdid,
                CreatedBy = Hdid,
                UpdatedBy = Hdid,
            };

            DbResult<UserFeedback> mockedDbResult = new()
            {
                Status = DbStatusCode.Error,
                Payload = userFeedback,
            };

            Mock<IUserFeedbackService> userFeedbackServiceMock = new();
            userFeedbackServiceMock.Setup(s => s.CreateUserFeedbackAsync(It.IsAny<Feedback>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockedDbResult);

            UserFeedbackController controller = new(userFeedbackServiceMock.Object);
            IActionResult actualResult = await controller.CreateUserFeedback(Hdid, feedback, default);

            Assert.IsType<ConflictResult>(actualResult);
        }

        /// <summary>
        /// CreateUserFeedback - BadRequestResult Error scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserFeedbackWithBadRequestResultError()
        {
            DbResult<UserFeedback> mockedDbResult = new()
            {
                Status = DbStatusCode.Error,
            };

            Mock<IUserFeedbackService> userFeedbackServiceMock = new();
            userFeedbackServiceMock.Setup(s => s.CreateUserFeedbackAsync(It.IsAny<Feedback>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockedDbResult);

            UserFeedbackController controller = new(userFeedbackServiceMock.Object);
            IActionResult actualResult = await controller.CreateUserFeedback(Hdid, null, default);

            Assert.IsType<BadRequestResult>(actualResult);
        }

        /// <summary>
        /// CreateRating - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateRating()
        {
            // Arrange
            SubmitRating rating = new()
            {
                RatingValue = 5,
                Skip = false,
            };

            RequestResult<RatingModel> expected = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new() { RatingValue = rating.RatingValue, Skip = rating.Skip },
            };

            Mock<IUserFeedbackService> userFeedbackServiceMock = new();
            userFeedbackServiceMock.Setup(s => s.CreateRatingAsync(It.IsAny<SubmitRating>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);
            UserFeedbackController controller = new(userFeedbackServiceMock.Object);

            // Act
            RequestResult<RatingModel> actual = await controller.CreateRating(rating, default);

            // Assert
            actual.ShouldDeepEqual(expected);
        }
    }
}
