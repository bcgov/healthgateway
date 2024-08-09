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
    using AutoMapper;
    using DeepEqual.Syntax;
    using Hangfire;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Utils;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserFeedbackService's Unit Tests.
    /// </summary>
    public class UserFeedbackServiceTests
    {
        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();
        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(Mapper, GetCryptoDelegateMock().Object, new Mock<IAuthenticationDelegate>().Object);

        /// <summary>
        /// CreateRating - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateRating()
        {
            // Arrange
            CreateRatingMock mock = SetupCreateRatingMock();

            // Act
            RequestResult<RatingModel> actual = await mock.Service.CreateRatingAsync(mock.Rating);

            // Assert
            actual.ShouldDeepEqual(mock.Expected);
        }

        /// <summary>
        /// CreateRating - Database Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateRatingWithError()
        {
            // Arrange
            CreateRatingMock mock = SetupCreateRatingMock(true);

            // Act
            RequestResult<RatingModel> actual = await mock.Service.CreateRatingAsync(mock.Rating);

            // Assert
            actual.ShouldDeepEqual(mock.Expected);
        }

        /// <summary>
        /// CreateUserFeedBack - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserFeedback()
        {
            // Arrange
            CreateUserFeedbackMock mock = SetupCreateUserFeedbackMock();

            // Act
            DbResult<UserFeedback> actual = await mock.Service.CreateUserFeedbackAsync(mock.Feedback, mock.Hdid);

            // Assert
            actual.ShouldDeepEqual(mock.Expected);
        }

        private static Mock<ICryptoDelegate> GetCryptoDelegateMock()
        {
            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.GenerateKey()).Returns(() => "Y1FmVGpXblpxNHQ3dyF6JUMqRi1KYU5kUmdVa1hwMnM=");
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));
            return cryptoDelegateMock;
        }

        private static CreateRatingMock SetupCreateRatingMock(bool dbErrorExists = false)
        {
            const string dbErrorMessage = "DB Error!";
            Guid ratingId = Guid.NewGuid();

            SubmitRating createRating = new()
            {
                RatingValue = 5,
                Skip = false,
            };

            Rating rating = new()
            {
                Id = ratingId,
                RatingValue = createRating.RatingValue,
                Skip = createRating.Skip,
            };

            DbResult<Rating> dbResult = new()
            {
                Payload = rating,
                Status = dbErrorExists ? DbStatusCode.Error : DbStatusCode.Created,
                Message = dbErrorExists ? dbErrorMessage : string.Empty,
            };

            RequestResult<RatingModel> expected = new()
            {
                ResultStatus = dbErrorExists ? ResultType.Error : ResultType.Success,
                ResultError = dbErrorExists
                    ? new()
                    {
                        ResultMessage = dbErrorMessage,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    }
                    : null,
                ResourcePayload = dbErrorExists ? null : new() { Id = ratingId, RatingValue = rating.RatingValue, Skip = rating.Skip },
            };

            Mock<IRatingDelegate> ratingDelegateMock = new();
            ratingDelegateMock.Setup(s => s.InsertRatingAsync(It.Is<Rating>(r => r.RatingValue == rating.RatingValue && r.Skip == rating.Skip), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbResult);

            IUserFeedbackService service = new UserFeedbackService(
                new Mock<ILogger<UserFeedbackService>>().Object,
                new Mock<IFeedbackDelegate>().Object,
                ratingDelegateMock.Object,
                new Mock<IUserProfileDelegate>().Object,
                new Mock<IBackgroundJobClient>().Object,
                MappingService,
                new Mock<IAuthenticationDelegate>().Object);

            return new(service, expected, createRating);
        }

        private static CreateUserFeedbackMock SetupCreateUserFeedbackMock()
        {
            const string hdid = "Mocked UserProfileId";
            const UserLoginClientType defaultClientType = UserLoginClientType.Mobile;

            UserFeedback userFeedback = new()
            {
                Id = Guid.NewGuid(),
                Comment = "Mocked Comment",
                UserProfileId = hdid,
                ClientCode = defaultClientType,
            };

            Feedback feedback = new()
            {
                Comment = userFeedback.Comment,
            };

            DbResult<UserFeedback> dbResult = new()
            {
                Payload = userFeedback,
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
                            r => r.Comment == userFeedback.Comment &&
                                 r.UserProfileId == userFeedback.UserProfileId),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbResult);

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.Is<string>(h => h == userFeedback.UserProfileId),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);

            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(
                    s => s.FetchAuthenticatedUserClientType())
                .Returns(defaultClientType);

            IUserFeedbackService service = new UserFeedbackService(
                new Mock<ILogger<UserFeedbackService>>().Object,
                userFeedbackDelegateMock.Object,
                new Mock<IRatingDelegate>().Object,
                userProfileDelegateMock.Object,
                new Mock<IBackgroundJobClient>().Object,
                MappingService,
                authenticationDelegateMock.Object);

            return new(service, dbResult, feedback, hdid);
        }

        private sealed record CreateRatingMock(IUserFeedbackService Service, RequestResult<RatingModel> Expected, SubmitRating Rating);

        private sealed record CreateUserFeedbackMock(IUserFeedbackService Service, DbResult<UserFeedback> Expected, Feedback Feedback, string Hdid);
    }
}
