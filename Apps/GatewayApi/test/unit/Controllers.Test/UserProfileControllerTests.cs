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
            RequestResult<UserProfileModel> actualResult = await this.GetUserProfile(expected, new Dictionary<string, UserPreferenceModel>()).ConfigureAwait(true);

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
            RequestResult<UserProfileModel> actualResult = await this.GetUserProfile(expected, null).ConfigureAwait(true);

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
            userProfileServiceMock.Setup(s => s.CreateUserProfile(createUserRequest, It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(expected);
            Mock<IUserEmailService> emailServiceMock = new();
            Mock<IUserSmsService> smsServiceMock = new();

            UserProfileController service = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);

            ActionResult<RequestResult<UserProfileModel>> actualResult = await service.CreateUserProfile(this.hdid, createUserRequest).ConfigureAwait(true);
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
            userProfileServiceMock.Setup(s => s.CreateUserProfile(createUserRequest, It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(expected);
            Mock<IUserEmailService> emailServiceMock = new();
            Mock<IUserSmsService> smsServiceMock = new();

            UserProfileController service = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);

            ActionResult<RequestResult<UserProfileModel>> actualResult = await service.CreateUserProfile(this.hdid, createUserRequest).ConfigureAwait(true);
            Assert.IsType<BadRequestResult>(actualResult.Result);
        }

        /// <summary>
        /// ValidateAge - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateAge()
        {
            PrimitiveRequestResult<bool> expected = new() { ResultStatus = ResultType.Success, ResourcePayload = true };
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new();
            userProfileServiceMock.Setup(s => s.ValidateMinimumAge(this.hdid)).ReturnsAsync(expected);

            UserProfileController controller = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSmsService>().Object,
                new Mock<IAuthenticationDelegate>().Object);

            RequestResult<bool> actualResult = await controller.Validate(this.hdid).ConfigureAwait(true);

            Assert.Equal(expected, actualResult);
        }

        /// <summary>
        /// CreateUserPreference - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldCreateUserPreference()
        {
            UserPreferenceModel userPref = new()
            {
                HdId = this.hdid,
                Preference = "tutorialMenuNote",
                Value = "Body value",
            };

            ActionResult<RequestResult<UserPreferenceModel>> actualResult = this.CreateUserPreference(userPref);

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
        [Fact]
        public void ShouldCreateUserPreferenceWithBadRequestResultError()
        {
            ActionResult<RequestResult<UserPreferenceModel>> actualResult = this.CreateUserPreference(null);

            Assert.IsType<BadRequestResult>(actualResult.Result);
        }

        /// <summary>
        /// UpdateUserPreference - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldUpdateUserPreference()
        {
            UserPreferenceModel userPref = new()
            {
                HdId = this.hdid,
                Preference = "tutorialMenuNote",
                Value = "Body value",
            };

            ActionResult<RequestResult<UserPreferenceModel>> actualResult = this.UpdateUserPreference(userPref);

            RequestResult<UserPreferenceModel>? reqResult = actualResult.Value;
            Assert.NotNull(reqResult);
            Assert.Equal(ResultType.Success, reqResult.ResultStatus);
        }

        /// <summary>
        /// UpdateUserPreference - Bad Request.
        /// </summary>
        [Fact]
        public void ShouldUpdateUserPreferenceWithBadRequestResultError()
        {
            ActionResult<RequestResult<UserPreferenceModel>> actualResult = this.UpdateUserPreference(null);

            Assert.IsType<BadRequestResult>(actualResult.Result);
        }

        /// <summary>
        /// UpdateUserPreference - Bad Request (Empty Preference).
        /// </summary>
        [Fact]
        public void ShouldUpdateUserPreferenceWithEmptyPreferenceError()
        {
            UserPreferenceModel userPref = new()
            {
                HdId = this.hdid,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                Preference = null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                Value = "Body value",
            };

            ActionResult<RequestResult<UserPreferenceModel>> actualResult = this.UpdateUserPreference(userPref);

            Assert.IsType<BadRequestResult>(actualResult.Result);
        }

        /// <summary>
        /// UpdateUserPreference - Forbidden Request.
        /// </summary>
        [Fact]
        public void ShouldUpdateUserPreferenceWithForbidResultError()
        {
            UserPreferenceModel userPref = new()
            {
                HdId = this.hdid + "dif.",
                Preference = "valid pref name",
                Value = "Body value",
            };

            ActionResult<RequestResult<UserPreferenceModel>> actualResult = this.UpdateUserPreference(userPref);

            Assert.IsType<ForbidResult>(actualResult.Result);
        }

        /// <summary>
        /// GetLastTermsOfService - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetLastTermsOfService()
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
            userProfileServiceMock.Setup(s => s.GetActiveTermsOfService()).Returns(expectedResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            Mock<IUserEmailService> emailServiceMock = new();
            Mock<IUserSmsService> smsServiceMock = new();

            UserProfileController service = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);

            RequestResult<TermsOfServiceModel> actualResult = service.GetLastTermsOfService();
            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// UpdateUserEmail - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldUpdateUserEmail()
        {
            Mock<IUserEmailService> emailServiceMock = new();
            emailServiceMock.Setup(s => s.UpdateUserEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new(
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                new Mock<IUserSmsService>().Object,
                new Mock<IAuthenticationDelegate>().Object);

            bool actualResult = controller.UpdateUserEmail(this.hdid, "emailadd@hgw.ca");

            Assert.True(actualResult);
        }

        /// <summary>
        /// ValidateEmail - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateEmail()
        {
            PrimitiveRequestResult<bool> primitiveRequestResult = new()
            {
                ResourcePayload = true,
                ResultStatus = ResultType.Success,
                ResultError = null,
            };

            Mock<IUserEmailService> emailServiceMock = new();
            emailServiceMock.Setup(s => s.ValidateEmail(It.IsAny<string>(), It.IsAny<Guid>())).Returns(primitiveRequestResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new(
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                new Mock<IUserSmsService>().Object,
                new Mock<IAuthenticationDelegate>().Object);

            ActionResult<PrimitiveRequestResult<bool>> actualResult = await controller.ValidateEmail(this.hdid, Guid.NewGuid()).ConfigureAwait(true);
            Assert.Equal(ResultType.Success, actualResult.Value?.ResultStatus);
        }

        /// <summary>
        /// ValidateEmail - Email not found error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateEmailWithEmailNotFound()
        {
            PrimitiveRequestResult<bool> primitiveRequestResult = new()
            {
                ResourcePayload = false,
                ResultStatus = ResultType.Error,
                ResultError = null,
            };
            Mock<IUserEmailService> emailServiceMock = new();
            emailServiceMock.Setup(s => s.ValidateEmail(It.IsAny<string>(), It.IsAny<Guid>())).Returns(primitiveRequestResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new(
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                new Mock<IUserSmsService>().Object,
                new Mock<IAuthenticationDelegate>().Object);

            ActionResult<PrimitiveRequestResult<bool>> actualResult = await controller.ValidateEmail(this.hdid, Guid.NewGuid()).ConfigureAwait(true);
            Assert.Equal(ResultType.Error, actualResult.Value?.ResultStatus);
        }

        /// <summary>
        /// UpdateUserSMSNumber - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldUpdateUserSmsNumber()
        {
            Mock<IUserSmsService> smsServiceMock = new();
            smsServiceMock.Setup(s => s.UpdateUserSms(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new(
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);

            bool actualResult = controller.UpdateUserSmsNumber(this.hdid, "250 123 456");
            Assert.True(actualResult);
        }

        /// <summary>
        /// ValidateSms - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldValidateSms()
        {
            PrimitiveRequestResult<bool> primitiveRequestResult = new()
            {
                ResourcePayload = true,
                ResultStatus = ResultType.Success,
                ResultError = null,
            };
            Mock<IUserSmsService> smsServiceMock = new();
            smsServiceMock.Setup(s => s.ValidateSms(It.IsAny<string>(), It.IsAny<string>())).Returns(primitiveRequestResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new(
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);

            ActionResult<PrimitiveRequestResult<bool>> actualResult = Task.Run(async () => await controller.ValidateSms(this.hdid, "205 123 4567").ConfigureAwait(true)).Result;

            PrimitiveRequestResult<bool>? result = actualResult.Value;
            Assert.Equal(ResultType.Success, result?.ResultStatus);
            Assert.Equal(true, result?.ResourcePayload);
        }

        /// <summary>
        /// ValidateSms - Sms not found error.
        /// </summary>
        [Fact]
        public void ShouldValidateSmsNotFoundResult()
        {
            PrimitiveRequestResult<bool> primitiveRequestResult = new()
            {
                ResourcePayload = false,
                ResultStatus = ResultType.Success,
                ResultError = null,
            };
            Mock<IUserSmsService> smsServiceMock = new();
            smsServiceMock.Setup(s => s.ValidateSms(It.IsAny<string>(), It.IsAny<string>())).Returns(primitiveRequestResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new(
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);

            ActionResult<PrimitiveRequestResult<bool>> actualResult = Task.Run(async () => await controller.ValidateSms(this.hdid, "205 123 4567").ConfigureAwait(true)).Result;

            PrimitiveRequestResult<bool>? result = actualResult.Value;
            Assert.Equal(ResultType.Success, result?.ResultStatus);
            Assert.Equal(false, result?.ResourcePayload);
        }

        /// <summary>
        /// Validates the controller update terms of service method.
        /// </summary>
        [Fact]
        public void ShouldUpdateTerms()
        {
            RequestResult<UserProfileModel> expected = new()
            {
                ResultStatus = ResultType.Success,
            };
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            Mock<IUserProfileService> userProfileServiceMock = new();
            userProfileServiceMock.Setup(s => s.UpdateAcceptedTerms(this.hdid, It.IsAny<Guid>())).Returns(expected);

            UserProfileController controller = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSmsService>().Object,
                new Mock<IAuthenticationDelegate>().Object);

            RequestResult<UserProfileModel> actualResult = controller.UpdateAcceptedTerms(this.hdid, Guid.Empty);
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
            userProfileServiceMock.Setup(s => s.GetUserProfile(this.hdid, It.IsAny<DateTime>())).Returns(Task.FromResult(expected));
            userProfileServiceMock.Setup(s => s.GetActiveTermsOfService()).Returns(new RequestResult<TermsOfServiceModel>());
            userProfileServiceMock.Setup(s => s.GetUserPreferences(this.hdid))
                .Returns(new RequestResult<Dictionary<string, UserPreferenceModel>> { ResourcePayload = userPreferencePayloadMock });

            Mock<IUserEmailService> emailServiceMock = new();
            Mock<IUserSmsService> smsServiceMock = new();

            UserProfileController service = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);
            return await service.GetUserProfile(this.hdid).ConfigureAwait(true);
        }

        private ActionResult<RequestResult<UserPreferenceModel>> UpdateUserPreference(UserPreferenceModel? userPref)
        {
            // Setup
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new();
            RequestResult<UserPreferenceModel> result = new()
            {
                ResourcePayload = userPref,
                ResultStatus = ResultType.Success,
            };

            userProfileServiceMock.Setup(s => s.UpdateUserPreference(userPref)).Returns(result);

            Mock<IUserEmailService> emailServiceMock = new();
            Mock<IUserSmsService> smsServiceMock = new();

            UserProfileController service = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);
            return service.UpdateUserPreference(this.hdid, userPref);
        }

        private ActionResult<RequestResult<UserPreferenceModel>> CreateUserPreference(UserPreferenceModel? userPref)
        {
            // Setup
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new();
            RequestResult<UserPreferenceModel> result = new()
            {
                ResourcePayload = userPref,
                ResultStatus = ResultType.Success,
            };

            userProfileServiceMock.Setup(s => s.CreateUserPreference(userPref)).Returns(result);

            Mock<IUserEmailService> emailServiceMock = new();
            Mock<IUserSmsService> smsServiceMock = new();

            UserProfileController service = new(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object,
                new Mock<IAuthenticationDelegate>().Object);
            return service.CreateUserPreference(this.hdid, userPref);
        }
    }
}
