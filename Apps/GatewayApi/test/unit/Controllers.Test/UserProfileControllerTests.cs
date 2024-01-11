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
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.GatewayApi.Controllers;
    using HealthGateway.GatewayApi.MapUtils;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Services.Test.Utils;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserProfileController's Unit Tests.
    /// </summary>
    public class UserProfileControllerTests
    {
        private readonly string hdid = "1234567890123456789012345678901234567890123456789012";
        private readonly string token = "Fake Access Token";
        private readonly string userId = "1001";

        /// <summary>
        /// GetUserProfile - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUserProfile()
        {
            RequestResult<UserProfileModel> expected = this.GetUserProfileExpectedRequestResultMock(ResultType.Success);
            RequestResult<UserProfileModel> actualResult = await this.GetUserProfile(expected, new Dictionary<string, UserPreferenceModel>());

            expected.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// GetUserProfile - With Empty User Preferences.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUserProfileWithoutUserPreference()
        {
            RequestResult<UserProfileModel> expected = this.GetUserProfileExpectedRequestResultMock(ResultType.Success);
            RequestResult<UserProfileModel> actualResult = await this.GetUserProfile(expected, null);

            Assert.NotNull(actualResult);
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Empty(actualResult.ResourcePayload?.Preferences);
        }

        /// <summary>
        /// CreateUserProfile - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserProfile()
        {
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd"),
            };

            CreateUserRequest createUserRequest = new()
            {
                Profile = userProfile,
            };

            RequestResult<UserProfileModel> expected = new()
            {
                ResourcePayload = UserProfileMapUtils.CreateFromDbModel(userProfile, userProfile.TermsOfServiceId, MapperUtil.InitializeAutoMapper()),
                ResultStatus = ResultType.Success,
            };

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new();
            userProfileServiceMock.Setup(s => s.CreateUserProfileAsync(createUserRequest, It.IsAny<DateTime>(), It.IsAny<string>(), CancellationToken.None)).ReturnsAsync(expected);
            Mock<IUserEmailService> emailServiceMock = new();
            Mock<IUserSmsService> smsServiceMock = new();

            UserProfileController service = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);

            ActionResult<RequestResult<UserProfileModel>> actualResult = await service.CreateUserProfile(this.hdid, createUserRequest, CancellationToken.None);
            expected.ShouldDeepEqual(actualResult.Value);
        }

        /// <summary>
        /// CreateUserProfile - Create User HDID doesn't match Token HDID.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserProfileBadRequest()
        {
            UserProfile userProfile = new()
            {
                HdId = "badhdid",
                TermsOfServiceId = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd"),
            };

            CreateUserRequest createUserRequest = new()
            {
                Profile = userProfile,
            };

            RequestResult<UserProfileModel> expected = new()
            {
                ResourcePayload = UserProfileMapUtils.CreateFromDbModel(userProfile, userProfile.TermsOfServiceId, MapperUtil.InitializeAutoMapper()),
                ResultStatus = ResultType.Success,
            };

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new();
            userProfileServiceMock.Setup(s => s.CreateUserProfileAsync(createUserRequest, It.IsAny<DateTime>(), It.IsAny<string>(), CancellationToken.None)).ReturnsAsync(expected);
            Mock<IUserEmailService> emailServiceMock = new();
            Mock<IUserSmsService> smsServiceMock = new();

            UserProfileController service = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);

            ActionResult<RequestResult<UserProfileModel>> actualResult = await service.CreateUserProfile(this.hdid, createUserRequest, CancellationToken.None);
            Assert.IsType<BadRequestResult>(actualResult.Result);
        }

        /// <summary>
        /// ValidateAge - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateAge()
        {
            RequestResult<bool> expected = new() { ResultStatus = ResultType.Success, ResourcePayload = true };
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new();
            userProfileServiceMock.Setup(s => s.ValidateMinimumAgeAsync(this.hdid, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

            UserProfileController controller = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSmsService>().Object,
                new Mock<IAuthenticationDelegate>().Object);

            RequestResult<bool> actualResult = await controller.Validate(this.hdid, It.IsAny<CancellationToken>());

            Assert.Equal(expected, actualResult);
        }

        /// <summary>
        /// CreateUserPreference - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserPreference()
        {
            UserPreferenceModel userPref = new()
            {
                HdId = this.hdid,
                Preference = "tutorialMenuNote",
                Value = "Body value",
            };

            ActionResult<RequestResult<UserPreferenceModel>> actualResult = await this.CreateUserPreference(userPref);

            RequestResult<UserPreferenceModel>? reqResult = actualResult.Value;
            Assert.NotNull(reqResult);
            Assert.Equal(ResultType.Success, reqResult.ResultStatus);
            Assert.Equal(this.hdid, reqResult.ResourcePayload!.HdId);
            Assert.Equal(this.hdid, reqResult.ResourcePayload.CreatedBy);
            Assert.Equal(this.hdid, reqResult.ResourcePayload.UpdatedBy);
        }

        /// <summary>
        /// CreateUserPreference - Bad Request.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserPreferenceWithBadRequestResultError()
        {
            ActionResult<RequestResult<UserPreferenceModel>> actualResult = await this.CreateUserPreference(null);

            Assert.IsType<BadRequestResult>(actualResult.Result);
        }

        /// <summary>
        /// UpdateUserPreference - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateUserPreference()
        {
            UserPreferenceModel userPref = new()
            {
                HdId = this.hdid,
                Preference = "tutorialMenuNote",
                Value = "Body value",
            };

            ActionResult<RequestResult<UserPreferenceModel>> actualResult = await this.UpdateUserPreference(userPref);

            RequestResult<UserPreferenceModel>? reqResult = actualResult.Value;
            Assert.NotNull(reqResult);
            Assert.Equal(ResultType.Success, reqResult.ResultStatus);
        }

        /// <summary>
        /// UpdateUserPreference - Bad Request.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateUserPreferenceWithBadRequestResultError()
        {
            ActionResult<RequestResult<UserPreferenceModel>> actualResult = await this.UpdateUserPreference(null);

            Assert.IsType<BadRequestResult>(actualResult.Result);
        }

        /// <summary>
        /// UpdateUserPreference - Bad Request (Empty Preference).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateUserPreferenceWithEmptyPreferenceError()
        {
            UserPreferenceModel userPref = new()
            {
                HdId = this.hdid,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                Preference = null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                Value = "Body value",
            };

            ActionResult<RequestResult<UserPreferenceModel>> actualResult = await this.UpdateUserPreference(userPref);

            Assert.IsType<BadRequestResult>(actualResult.Result);
        }

        /// <summary>
        /// UpdateUserPreference - Forbidden Request.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateUserPreferenceWithForbidResultError()
        {
            UserPreferenceModel userPref = new()
            {
                HdId = this.hdid + "dif.",
                Preference = "valid pref name",
                Value = "Body value",
            };

            ActionResult<RequestResult<UserPreferenceModel>> actualResult = await this.UpdateUserPreference(userPref);

            Assert.IsType<ForbidResult>(actualResult.Result);
        }

        /// <summary>
        /// GetLastTermsOfService - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetLastTermsOfService()
        {
            // Setup
            TermsOfServiceModel termsOfService = new()
            {
                Id = Guid.NewGuid(),
                Content = "abc",
                EffectiveDate = DateTime.Today,
            };
            RequestResult<TermsOfServiceModel> expectedResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = termsOfService,
            };

            Mock<IUserProfileService> userProfileServiceMock = new();
            userProfileServiceMock.Setup(s => s.GetActiveTermsOfServiceAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            Mock<IUserEmailService> emailServiceMock = new();
            Mock<IUserSmsService> smsServiceMock = new();

            UserProfileController service = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);

            RequestResult<TermsOfServiceModel> actualResult = await service.GetLastTermsOfService(It.IsAny<CancellationToken>());
            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// UpdateUserEmailAsync - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateUserEmail()
        {
            Mock<IUserEmailService> emailServiceMock = new();
            emailServiceMock.Setup(s => s.UpdateUserEmailAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None)).ReturnsAsync(true);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new(
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                new Mock<IUserSmsService>().Object,
                new Mock<IAuthenticationDelegate>().Object);

            bool actualResult = await controller.UpdateUserEmail(this.hdid, "emailadd@hgw.ca", default);

            Assert.True(actualResult);
        }

        /// <summary>
        /// ValidateEmailAsync - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateEmail()
        {
            RequestResult<bool> requestResult = new()
            {
                ResourcePayload = true,
                ResultStatus = ResultType.Success,
                ResultError = null,
            };

            Mock<IUserEmailService> emailServiceMock = new();
            emailServiceMock.Setup(s => s.ValidateEmailAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);

            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate.Setup(s => s.FetchAuthenticatedUserTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(this.token);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new(
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                new Mock<IUserSmsService>().Object,
                mockAuthenticationDelegate.Object);

            ActionResult<RequestResult<bool>> actualResult = await controller.ValidateEmail(this.hdid, Guid.NewGuid(), default);
            Assert.Equal(ResultType.Success, actualResult.Value?.ResultStatus);
        }

        /// <summary>
        /// ValidateEmailAsync - Email not found error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateEmailWithEmailNotFound()
        {
            RequestResult<bool> requestResult = new()
            {
                ResourcePayload = false,
                ResultStatus = ResultType.Error,
                ResultError = null,
            };
            Mock<IUserEmailService> emailServiceMock = new();
            emailServiceMock.Setup(s => s.ValidateEmailAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);

            Mock<IAuthenticationDelegate> mockAuthenticationDelegate = new();
            mockAuthenticationDelegate.Setup(s => s.FetchAuthenticatedUserTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(this.token);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new(
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                new Mock<IUserSmsService>().Object,
                mockAuthenticationDelegate.Object);

            ActionResult<RequestResult<bool>> actualResult = await controller.ValidateEmail(this.hdid, Guid.NewGuid(), default);
            Assert.Equal(ResultType.Error, actualResult.Value?.ResultStatus);
        }

        /// <summary>
        /// UpdateUserSMSNumber - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateUserSmsNumber()
        {
            Mock<IUserSmsService> smsServiceMock = new();
            smsServiceMock.Setup(s => s.UpdateUserSmsAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None)).ReturnsAsync(true);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new(
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);

            bool actualResult = await controller.UpdateUserSmsNumberAsync(this.hdid, "250 123 456", default);
            Assert.True(actualResult);
        }

        /// <summary>
        /// ValidateSmsAsync - Happy Path.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldValidateSms()
        {
            RequestResult<bool> requestResult = new()
            {
                ResourcePayload = true,
                ResultStatus = ResultType.Success,
                ResultError = null,
            };
            Mock<IUserSmsService> smsServiceMock = new();
            smsServiceMock.Setup(s => s.ValidateSmsAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None)).ReturnsAsync(requestResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new(
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);

            ActionResult<RequestResult<bool>> actualResult = await controller.ValidateSms(this.hdid, "205 123 4567", CancellationToken.None);

            RequestResult<bool>? result = actualResult.Value;
            Assert.Equal(ResultType.Success, result?.ResultStatus);
            Assert.Equal(true, result?.ResourcePayload);
        }

        /// <summary>
        /// ValidateSmsAsync - Sms not found error.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ShouldValidateSmsNotFoundResult()
        {
            RequestResult<bool> requestResult = new()
            {
                ResourcePayload = false,
                ResultStatus = ResultType.Success,
                ResultError = null,
            };
            Mock<IUserSmsService> smsServiceMock = new();
            smsServiceMock.Setup(s => s.ValidateSmsAsync(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None)).ReturnsAsync(requestResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new(
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);

            ActionResult<RequestResult<bool>> actualResult = await controller.ValidateSms(this.hdid, "205 123 4567", CancellationToken.None);

            RequestResult<bool>? result = actualResult.Value;
            Assert.Equal(ResultType.Success, result?.ResultStatus);
            Assert.Equal(false, result?.ResourcePayload);
        }

        /// <summary>
        /// Validates the controller update terms of service method.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateTerms()
        {
            RequestResult<UserProfileModel> expected = new()
            {
                ResultStatus = ResultType.Success,
            };
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            Mock<IUserProfileService> userProfileServiceMock = new();
            userProfileServiceMock.Setup(s => s.UpdateAcceptedTermsAsync(this.hdid, It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

            UserProfileController controller = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSmsService>().Object,
                new Mock<IAuthenticationDelegate>().Object);

            RequestResult<UserProfileModel> actualResult = await controller.UpdateAcceptedTerms(this.hdid, Guid.Empty, It.IsAny<CancellationToken>());
            expected.ShouldDeepEqual(actualResult);
        }

        private static Mock<IHttpContextAccessor> CreateValidHttpContext(string token, string userId, string hdid)
        {
            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", token },
                { "referer", "http://localhost/" },
            };
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("hdid", hdid),
                new Claim("auth_time", "123"),
                new Claim("access_token", token),
            };
            ClaimsIdentity identity = new(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new(identity);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);
            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);
            Mock<IAuthenticationService> authenticationMock = new();
            AuthenticateResult authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties.StoreTokens(
                new[]
                {
                    new AuthenticationToken { Name = "access_token", Value = token },
                });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            httpContextAccessorMock
                .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            return httpContextAccessorMock;
        }

        private RequestResult<UserProfileModel> GetUserProfileExpectedRequestResultMock(ResultType resultType)
        {
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd"),
            };

            return new RequestResult<UserProfileModel>
            {
                ResourcePayload = UserProfileMapUtils.CreateFromDbModel(userProfile, userProfile.TermsOfServiceId, MapperUtil.InitializeAutoMapper()),
                ResultStatus = resultType,
            };
        }

        private async Task<RequestResult<UserProfileModel>> GetUserProfile(
            RequestResult<UserProfileModel> expected,
            Dictionary<string, UserPreferenceModel>? userPreferencePayloadMock)
        {
            // Setup
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new();
            userProfileServiceMock.Setup(s => s.GetUserProfileAsync(this.hdid, It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);
            userProfileServiceMock.Setup(s => s.GetActiveTermsOfServiceAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new RequestResult<TermsOfServiceModel>());
            userProfileServiceMock.Setup(s => s.GetUserPreferencesAsync(this.hdid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RequestResult<Dictionary<string, UserPreferenceModel>> { ResourcePayload = userPreferencePayloadMock });

            Mock<IUserEmailService> emailServiceMock = new();
            Mock<IUserSmsService> smsServiceMock = new();

            UserProfileController controller = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);
            return await controller.GetUserProfile(this.hdid, It.IsAny<CancellationToken>());
        }

        private async Task<ActionResult<RequestResult<UserPreferenceModel>>> UpdateUserPreference(UserPreferenceModel? userPref)
        {
            // Setup
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new();
            RequestResult<UserPreferenceModel> requestResult = new()
            {
                ResourcePayload = userPref,
                ResultStatus = ResultType.Success,
            };

            userProfileServiceMock.Setup(s => s.UpdateUserPreferenceAsync(userPref, It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);

            Mock<IUserEmailService> emailServiceMock = new();
            Mock<IUserSmsService> smsServiceMock = new();

            UserProfileController controller = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);
            return await controller.UpdateUserPreference(this.hdid, userPref, It.IsAny<CancellationToken>());
        }

        private async Task<ActionResult<RequestResult<UserPreferenceModel>>> CreateUserPreference(UserPreferenceModel? userPref)
        {
            // Setup
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new();
            RequestResult<UserPreferenceModel> requestResult = new()
            {
                ResourcePayload = userPref,
                ResultStatus = ResultType.Success,
            };

            userProfileServiceMock.Setup(s => s.CreateUserPreferenceAsync(userPref, It.IsAny<CancellationToken>())).ReturnsAsync(requestResult);

            Mock<IUserEmailService> emailServiceMock = new();
            Mock<IUserSmsService> smsServiceMock = new();

            UserProfileController controller = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);
            return await controller.CreateUserPreference(this.hdid, userPref, It.IsAny<CancellationToken>());
        }
    }
}
