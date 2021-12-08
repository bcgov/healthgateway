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
    using System;
    using DeepEqual.Syntax;
    using Hangfire;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Services;
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
        [Fact]
        public void ShouldCreateRating()
        {
            Rating expectedRating = new()
            {
                RatingValue = 5,
                Skip = false,
            };

            DBResult<Rating> insertResult = new()
            {
                Payload = expectedRating,
                Status = DBStatusCode.Created,
            };

            Mock<IRatingDelegate> ratingDelegateMock = new();
            ratingDelegateMock.Setup(s => s.InsertRating(It.Is<Rating>(r => r.RatingValue == expectedRating.RatingValue && r.Skip == expectedRating.Skip))).Returns(insertResult);

            Mock<IBackgroundJobClient> mockJobclient = new();
            Mock<IUserProfileDelegate> mockProfileDelegate = new();

            IUserFeedbackService service = new UserFeedbackService(
                new Mock<ILogger<UserFeedbackService>>().Object,
                new Mock<IFeedbackDelegate>().Object,
                ratingDelegateMock.Object,
                mockProfileDelegate.Object,
                mockJobclient.Object);

            RequestResult<Rating> actualResult = service.CreateRating(expectedRating);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload?.IsDeepEqual(expectedRating));
        }

        /// <summary>
        /// CreateRating - Database Error.
        /// </summary>
        [Fact]
        public void ShouldCreateRatingWithError()
        {
            Rating expectedRating = new()
            {
                RatingValue = 5,
                Skip = false,
            };

            DBResult<Rating> insertResult = new()
            {
                Payload = expectedRating,
                Status = DBStatusCode.Error,
            };

            Mock<IRatingDelegate> ratingDelegateMock = new();
            ratingDelegateMock.Setup(s => s.InsertRating(It.Is<Rating>(r => r.RatingValue == expectedRating.RatingValue && r.Skip == expectedRating.Skip))).Returns(insertResult);

            Mock<IBackgroundJobClient> mockJobclient = new();
            Mock<IUserProfileDelegate> mockProfileDelegate = new();

            IUserFeedbackService service = new UserFeedbackService(
                new Mock<ILogger<UserFeedbackService>>().Object,
                new Mock<IFeedbackDelegate>().Object,
                ratingDelegateMock.Object,
                mockProfileDelegate.Object,
                mockJobclient.Object);

            RequestResult<Rating> actualResult = service.CreateRating(expectedRating);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// CreateUserFeedBack - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldCreateUserFeedback()
        {
            UserFeedback expectedUserFeedback = new()
            {
                Comment = "Mocked Comment",
                Id = Guid.NewGuid(),
                UserProfileId = "Mocked UserProfileId",
                IsSatisfied = true,
                IsReviewed = true,
            };

            DBResult<UserFeedback> insertResult = new()
            {
                Payload = expectedUserFeedback,
                Status = DBStatusCode.Created,
            };

            UserProfile profile = new()
            {
                Email = "mock@email.com",
            };

            DBResult<UserProfile> profileResult = new()
            {
                Payload = profile,
                Status = DBStatusCode.Read,
            };

            Mock<IFeedbackDelegate> userFeedbackDelegateMock = new();
            userFeedbackDelegateMock.Setup(s => s.InsertUserFeedback(It.Is<UserFeedback>(r => r.Comment == expectedUserFeedback.Comment && r.Id == expectedUserFeedback.Id && r.UserProfileId == expectedUserFeedback.UserProfileId && r.IsSatisfied == expectedUserFeedback.IsSatisfied && r.IsReviewed == expectedUserFeedback.IsReviewed))).Returns(insertResult);

            Mock<IBackgroundJobClient> mockJobclient = new();
            Mock<IUserProfileDelegate> mockProfileDelegate = new();
            mockProfileDelegate.Setup(s => s.GetUserProfile(It.Is<string>(hdid => hdid == expectedUserFeedback.UserProfileId))).Returns(profileResult);

            IUserFeedbackService service = new UserFeedbackService(
                new Mock<ILogger<UserFeedbackService>>().Object,
                userFeedbackDelegateMock.Object,
                new Mock<IRatingDelegate>().Object,
                mockProfileDelegate.Object,
                mockJobclient.Object);

            DBResult<UserFeedback> actualResult = service.CreateUserFeedback(expectedUserFeedback);

            Assert.Equal(DBStatusCode.Created, actualResult.Status);
            Assert.True(actualResult.Payload?.IsDeepEqual(expectedUserFeedback));
        }
    }
}
