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
        private const string Hdid = "Mocked UserProfileId";
        private const UserLoginClientType DefaultClientType = UserLoginClientType.Mobile;

        private static readonly IMapper Mapper = MapperUtil.InitializeAutoMapper();
        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(Mapper, GetCryptoDelegateMock().Object);

        /// <summary>
        /// CreateRating - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateRating()
        {
            // Arrange
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

            DbResult<Rating> createRatingResult = new()
            {
                Payload = rating,
                Status = DbStatusCode.Created,
            };

            RequestResult<RatingModel> expected = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new() { Id = ratingId, RatingValue = rating.RatingValue, Skip = rating.Skip },
            };

            IUserFeedbackService service = SetupCreateRatingMock(createRatingResult, createRating);

            // Act
            RequestResult<RatingModel> actual = await service.CreateRatingAsync(createRating);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// CreateRating - Database Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateRatingWithError()
        {
            // Arrange
            const string dbErrorMessage = "DB Error!";

            SubmitRating createRating = new()
            {
                RatingValue = 5,
                Skip = false,
            };

            DbResult<Rating> createRatingResult = new()
            {
                Status = DbStatusCode.Error,
                Message = dbErrorMessage,
            };

            RequestResult<RatingModel> expected = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new()
                {
                    ResultMessage = dbErrorMessage,
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                },
            };

            IUserFeedbackService service = SetupCreateRatingMock(createRatingResult, createRating);

            // Act
            RequestResult<RatingModel> actual = await service.CreateRatingAsync(createRating);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// CreateUserFeedBack - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserFeedback()
        {
            // Arrange
            UserFeedback createUserFeedback = new()
            {
                Id = Guid.NewGuid(),
                Comment = "Mocked Comment",
                UserProfileId = Hdid,
                ClientCode = DefaultClientType,
            };

            Feedback feedback = new()
            {
                Comment = createUserFeedback.Comment,
            };

            DbResult<UserFeedback> expected = new()
            {
                Payload = createUserFeedback,
                Status = DbStatusCode.Created,
            };

            IUserFeedbackService service = SetupCreateUserFeedbackMock(expected, createUserFeedback);

            // Act
            DbResult<UserFeedback> actual = await service.CreateUserFeedbackAsync(feedback, Hdid);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        private static Mock<ICryptoDelegate> GetCryptoDelegateMock()
        {
            Mock<ICryptoDelegate> cryptoDelegateMock = new();
            cryptoDelegateMock.Setup(s => s.GenerateKey()).Returns(() => "Y1FmVGpXblpxNHQ3dyF6JUMqRi1KYU5kUmdVa1hwMnM=");
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));
            return cryptoDelegateMock;
        }

        private static IUserFeedbackService GetUserFeedbackService(
            Mock<IFeedbackDelegate>? feedbackDelegateMock = null,
            Mock<IRatingDelegate>? ratingDelegateMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<IBackgroundJobClient>? backgroundJobClientMock = null,
            Mock<IAuthenticationDelegate>? authenticationDelegateMock = null)
        {
            feedbackDelegateMock ??= new();
            ratingDelegateMock ??= new();
            userProfileDelegateMock ??= new();
            backgroundJobClientMock ??= new();
            authenticationDelegateMock ??= new();

            return new UserFeedbackService(
                new Mock<ILogger<UserFeedbackService>>().Object,
                feedbackDelegateMock.Object,
                ratingDelegateMock.Object,
                userProfileDelegateMock.Object,
                backgroundJobClientMock.Object,
                MappingService,
                authenticationDelegateMock.Object);
        }

        private static IUserFeedbackService SetupCreateRatingMock(DbResult<Rating> dbResult, SubmitRating createRating)
        {
            Mock<IRatingDelegate> ratingDelegateMock = new();
            ratingDelegateMock.Setup(
                    s => s.InsertRatingAsync(
                        It.Is<Rating>(r => r.RatingValue == createRating.RatingValue && r.Skip == createRating.Skip),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbResult);

            return GetUserFeedbackService(ratingDelegateMock: ratingDelegateMock);
        }

        private static IUserFeedbackService SetupCreateUserFeedbackMock(DbResult<UserFeedback> dbResult, UserFeedback createUserFeedback)
        {
            UserProfile profile = new()
            {
                Email = "mock@email.com",
            };

            Mock<IFeedbackDelegate> userFeedbackDelegateMock = new();
            userFeedbackDelegateMock.Setup(
                    s => s.InsertUserFeedbackAsync(
                        It.Is<UserFeedback>(
                            r => r.Comment == createUserFeedback.Comment &&
                                 r.UserProfileId == createUserFeedback.UserProfileId),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbResult);

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.Is<string>(h => h == createUserFeedback.UserProfileId),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);

            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(
                    s => s.FetchAuthenticatedUserClientType())
                .Returns(DefaultClientType);

            return GetUserFeedbackService(
                userFeedbackDelegateMock,
                userProfileDelegateMock: userProfileDelegateMock,
                authenticationDelegateMock: authenticationDelegateMock);
        }
    }
}
