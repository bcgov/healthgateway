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
    using System.Collections.Generic;
    using System.Security.Claims;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Controllers;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
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
        [Fact]
        public void ShouldCreateUserFeedback()
        {
            var userFeedback = new UserFeedback()
            {
                UserProfileId = Hdid,
                CreatedBy = Hdid,
                UpdatedBy = Hdid,
            };

            DBResult<UserFeedback> mockedDBResult = new DBResult<UserFeedback>()
            {
                Status = Database.Constants.DBStatusCode.Created,
                Payload = userFeedback,
            };

            Mock<IUserFeedbackService> userFeedbackServiceMock = new Mock<IUserFeedbackService>();
            userFeedbackServiceMock.Setup(s => s.CreateUserFeedback(It.IsAny<UserFeedback>())).Returns(mockedDBResult);

            UserFeedbackController controller = new UserFeedbackController(userFeedbackServiceMock.Object);
            var actualResult = controller.CreateUserFeedback(Hdid, userFeedback);
            Assert.IsType<OkResult>(actualResult);
        }

        /// <summary>
        /// CreateUserFeedback - ConflictResult Error scenario.
        /// </summary>
        [Fact]
        public void ShouldCreateUserFeedbackWithConflictResultError()
        {
            var userFeedback = new UserFeedback()
            {
                UserProfileId = Hdid,
                CreatedBy = Hdid,
                UpdatedBy = Hdid,
            };

            DBResult<UserFeedback> mockedDBResult = new DBResult<UserFeedback>()
            {
                Status = Database.Constants.DBStatusCode.Error,
                Payload = userFeedback,
            };

            Mock<IUserFeedbackService> userFeedbackServiceMock = new Mock<IUserFeedbackService>();
            userFeedbackServiceMock.Setup(s => s.CreateUserFeedback(It.IsAny<UserFeedback>())).Returns(mockedDBResult);

            UserFeedbackController controller = new UserFeedbackController(userFeedbackServiceMock.Object);
            var actualResult = controller.CreateUserFeedback(Hdid, userFeedback);
            Assert.IsType<ConflictResult>(actualResult);
        }

        /// <summary>
        /// CreateUserFeedback - BadRequestResult Error scenario.
        /// </summary>
        [Fact]
        public void ShouldCreateUserFeedbackWithBadRequestResultError()
        {
            DBResult<UserFeedback> mockedDBResult = new DBResult<UserFeedback>()
            {
                Status = Database.Constants.DBStatusCode.Error,
            };

            Mock<IUserFeedbackService> userFeedbackServiceMock = new Mock<IUserFeedbackService>();
            userFeedbackServiceMock.Setup(s => s.CreateUserFeedback(It.IsAny<UserFeedback>())).Returns(mockedDBResult);

            UserFeedbackController controller = new UserFeedbackController(userFeedbackServiceMock.Object);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var actualResult = controller.CreateUserFeedback(Hdid, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.IsType<BadRequestResult>(actualResult);
        }

        /// <summary>
        /// CreateRating - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldCreateRating()
        {
            var rating = new Rating()
            {
                CreatedBy = Hdid,
                UpdatedBy = Hdid,
            };

            RequestResult<Rating> expectedResult = new RequestResult<Rating>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = rating,
            };

            Mock<IUserFeedbackService> userFeedbackServiceMock = new Mock<IUserFeedbackService>();
            userFeedbackServiceMock.Setup(s => s.CreateRating(It.IsAny<Rating>())).Returns(expectedResult);

            UserFeedbackController controller = new UserFeedbackController(userFeedbackServiceMock.Object);
            var actualResult = controller.CreateRating(rating);
            Assert.True(((JsonResult)actualResult).Value.IsDeepEqual(expectedResult));
        }
    }
}
