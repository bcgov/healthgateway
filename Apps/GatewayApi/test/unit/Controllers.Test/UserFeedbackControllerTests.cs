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
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Controllers;
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
            userFeedbackServiceMock.Setup(s => s.CreateUserFeedbackAsync(It.IsAny<UserFeedback>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockedDbResult);

            UserFeedbackController controller = new(userFeedbackServiceMock.Object);
            IActionResult actualResult = await controller.CreateUserFeedback(Hdid, userFeedback);

            Assert.IsType<OkResult>(actualResult);
        }

        /// <summary>
        /// CreateUserFeedback - ConflictResult Error scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserFeedbackWithConflictResultError()
        {
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
            userFeedbackServiceMock.Setup(s => s.CreateUserFeedbackAsync(It.IsAny<UserFeedback>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockedDbResult);

            UserFeedbackController controller = new(userFeedbackServiceMock.Object);
            IActionResult actualResult = await controller.CreateUserFeedback(Hdid, userFeedback);

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
            userFeedbackServiceMock.Setup(s => s.CreateUserFeedbackAsync(It.IsAny<UserFeedback>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockedDbResult);

            UserFeedbackController controller = new(userFeedbackServiceMock.Object);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            IActionResult actualResult = await controller.CreateUserFeedback(Hdid, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            Assert.IsType<BadRequestResult>(actualResult);
        }

        /// <summary>
        /// CreateRating - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateRating()
        {
            Rating rating = new()
            {
                CreatedBy = Hdid,
                UpdatedBy = Hdid,
            };

            RequestResult<Rating> expectedResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = rating,
            };

            Mock<IUserFeedbackService> userFeedbackServiceMock = new();
            userFeedbackServiceMock.Setup(s => s.CreateRatingAsync(It.IsAny<Rating>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            UserFeedbackController controller = new(userFeedbackServiceMock.Object);
            RequestResult<Rating> actualResult = await controller.CreateRating(rating, It.IsAny<CancellationToken>());

            expectedResult.ShouldDeepEqual(actualResult);
        }
    }
}
