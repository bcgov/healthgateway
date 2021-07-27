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
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;
    using HealthGateway.WebClient.Controllers;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
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
        [Fact]
        public void ShouldGetUserProfile()
        {
            RequestResult<UserProfileModel> expected = this.GetUserProfileExpectedRequestResultMock(ResultType.Success);
            IActionResult actualResult = this.GetUserProfile(expected, new Dictionary<string, UserPreferenceModel>() { });

            Assert.IsType<JsonResult>(actualResult);
            Assert.True(((JsonResult)actualResult)?.Value?.IsDeepEqual(expected));
        }

        /// <summary>
        /// GetUserProfile - With Empty User Preferences.
        /// </summary>
        [Fact]
        public void ShouldGetUserProfileWithoutUserPreference()
        {
            RequestResult<UserProfileModel> expected = this.GetUserProfileExpectedRequestResultMock(ResultType.Success);
            IActionResult actualResult = this.GetUserProfile(expected, null);

            Assert.IsType<JsonResult>(actualResult);
            var reqResult = (RequestResult<UserProfileModel>)((JsonResult)actualResult).Value;
            Assert.Equal(ResultType.Success, reqResult.ResultStatus);
            Assert.Empty(reqResult.ResourcePayload?.Preferences);
        }

        /// <summary>
        /// CreateUserProfile - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserProfile()
        {
            UserProfile userProfile = new UserProfile
            {
                HdId = this.hdid,
                AcceptedTermsOfService = true,
            };

            CreateUserRequest createUserRequest = new CreateUserRequest
            {
                Profile = userProfile,
            };

            RequestResult<UserProfileModel> expected = new RequestResult<UserProfileModel>
            {
                ResourcePayload = UserProfileModel.CreateFromDbModel(userProfile),
                ResultStatus = ResultType.Success,
            };

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new Mock<IUserProfileService>();
            userProfileServiceMock.Setup(s => s.CreateUserProfile(createUserRequest, It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(expected);
            Mock<IUserEmailService> emailServiceMock = new Mock<IUserEmailService>();
            Mock<IUserSMSService> smsServiceMock = new Mock<IUserSMSService>();

            UserProfileController service = new UserProfileController(
                new Mock<ILogger<UserProfileController>>().Object,
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object);
            IActionResult actualResult = await service.CreateUserProfile(this.hdid, createUserRequest).ConfigureAwait(true);

            Assert.IsType<JsonResult>(actualResult);
            Assert.True(((JsonResult)actualResult).Value?.IsDeepEqual(expected));
        }

        /// <summary>
        /// ValidateAge - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateAge()
        {
            PrimitiveRequestResult<bool> expected = new PrimitiveRequestResult<bool>() { ResultStatus = ResultType.Success, ResourcePayload = true };
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new Mock<IUserProfileService>();
            userProfileServiceMock.Setup(s => s.ValidateMinimumAge(this.hdid)).ReturnsAsync(expected);

            UserProfileController controller = new UserProfileController(
                new Mock<ILogger<UserProfileController>>().Object,
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object);
            IActionResult actualResult = await controller.Validate(this.hdid).ConfigureAwait(true);

            Assert.IsType<JsonResult>(actualResult);
            Assert.Equal(expected, ((JsonResult)actualResult).Value);
        }

        /// <summary>
        /// CreateUserPreference - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldCreateUserPreference()
        {
            var userPref = new UserPreferenceModel()
            {
                HdId = this.hdid,
                Preference = "actionedCovidModalAt",
                Value = "Body value",
            };
            IActionResult actualResult = this.CreateUserPreference(userPref);

            Assert.IsType<JsonResult>(actualResult);
            RequestResult<UserPreferenceModel>? reqResult = ((JsonResult)actualResult).Value as RequestResult<UserPreferenceModel>;
            Assert.NotNull(reqResult);
            Assert.Equal(ResultType.Success, reqResult!.ResultStatus);
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
            IActionResult actualResult = this.CreateUserPreference(null);

            Assert.IsType<BadRequestResult>(actualResult);
        }

        /// <summary>
        /// UpdateUserPreference - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldUpdateUserPreference()
        {
            var userPref = new UserPreferenceModel()
            {
                HdId = this.hdid,
                Preference = "actionedCovidModalAt",
                Value = "Body value",
            };
            IActionResult actualResult = this.UpdateUserPreference(userPref);

            Assert.IsType<JsonResult>(actualResult);
            RequestResult<UserPreferenceModel>? reqResult = ((JsonResult)actualResult).Value as RequestResult<UserPreferenceModel>;
            Assert.NotNull(reqResult);
            Assert.Equal(ResultType.Success, reqResult!.ResultStatus);
        }

        /// <summary>
        /// UpdateUserPreference - Bad Request.
        /// </summary>
        [Fact]
        public void ShouldUpdateUserPreferenceWithBadRequestResultError()
        {
            IActionResult actualResult = this.UpdateUserPreference(null);

            Assert.IsType<BadRequestResult>(actualResult);
        }

        /// <summary>
        /// UpdateUserPreference - Bad Request (Empty Preference).
        /// </summary>
        [Fact]
        public void ShouldUpdateUserPreferenceWithEmptyPreferenceError()
        {
            var userPref = new UserPreferenceModel()
            {
                HdId = this.hdid,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                Preference = null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                Value = "Body value",
            };
            IActionResult actualResult = this.UpdateUserPreference(userPref);

            Assert.IsType<BadRequestResult>(actualResult);
        }

        /// <summary>
        /// UpdateUserPreference - Forbidden Request.
        /// </summary>
        [Fact]
        public void ShouldUpdateUserPreferenceWithForbidResultError()
        {
            var userPref = new UserPreferenceModel()
            {
                HdId = this.hdid + "dif.",
                Preference = "valid pref name",
                Value = "Body value",
            };
            IActionResult actualResult = this.UpdateUserPreference(userPref);

            Assert.IsType<ForbidResult>(actualResult);
        }

        /// <summary>
        /// GetLastTermsOfService - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetLastTermsOfService()
        {
            // Setup
            var termsOfService = new TermsOfServiceModel()
            {
                Id = Guid.NewGuid(),
                Content = "abc",
                EffectiveDate = DateTime.Today,
            };
            RequestResult<TermsOfServiceModel> expectedResult = new RequestResult<TermsOfServiceModel>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = termsOfService,
            };

            Mock<IUserProfileService> userProfileServiceMock = new Mock<IUserProfileService>();
            userProfileServiceMock.Setup(s => s.GetActiveTermsOfService()).Returns(expectedResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            Mock<IUserEmailService> emailServiceMock = new Mock<IUserEmailService>();
            Mock<IUserSMSService> smsServiceMock = new Mock<IUserSMSService>();

            UserProfileController service = new UserProfileController(
                new Mock<ILogger<UserProfileController>>().Object,
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object);

            var actualResult = service.GetLastTermsOfService();

            Assert.IsType<JsonResult>(actualResult);
            Assert.True(((JsonResult)actualResult).Value?.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// UpdateUserEmail - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldUpdateUserEmail()
        {
            Mock<IUserEmailService> emailServiceMock = new Mock<IUserEmailService>();
            emailServiceMock.Setup(s => s.UpdateUserEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new UserProfileController(
                new Mock<ILogger<UserProfileController>>().Object,
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                new Mock<IUserSMSService>().Object);
            IActionResult actualResult = controller.UpdateUserEmail(this.hdid, "emailadd@hgw.ca");

            Assert.True((bool)((JsonResult)actualResult).Value);
        }

        /// <summary>
        /// ValidateEmail - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateEmail()
        {
            PrimitiveRequestResult<bool> primitiveRequestResult = new PrimitiveRequestResult<bool>()
            {
                ResourcePayload = true,
                ResultStatus = ResultType.Success,
                ResultError = null,
            };

            Mock<IUserEmailService> emailServiceMock = new Mock<IUserEmailService>();
            emailServiceMock.Setup(s => s.ValidateEmail(It.IsAny<string>(), It.IsAny<Guid>())).Returns(primitiveRequestResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new UserProfileController(
                new Mock<ILogger<UserProfileController>>().Object,
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                new Mock<IUserSMSService>().Object);
            IActionResult actualResult = await controller.ValidateEmail(this.hdid, Guid.NewGuid()).ConfigureAwait(true);
            var result = ((JsonResult)actualResult).Value as PrimitiveRequestResult<bool>;

            Assert.Equal(ResultType.Success, result?.ResultStatus);
        }

        /// <summary>
        /// ValidateEmail - Email not found error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateEmailWithEmailNotFound()
        {
            PrimitiveRequestResult<bool> primitiveRequestResult = new PrimitiveRequestResult<bool>()
            {
                ResourcePayload = false,
                ResultStatus = ResultType.Error,
                ResultError = null,
            };
            Mock<IUserEmailService> emailServiceMock = new Mock<IUserEmailService>();
            emailServiceMock.Setup(s => s.ValidateEmail(It.IsAny<string>(), It.IsAny<Guid>())).Returns(primitiveRequestResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new UserProfileController(
                new Mock<ILogger<UserProfileController>>().Object,
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                new Mock<IUserSMSService>().Object);
            IActionResult actualResult = await controller.ValidateEmail(this.hdid, Guid.NewGuid()).ConfigureAwait(true);
            var result = ((JsonResult)actualResult).Value as PrimitiveRequestResult<bool>;

            Assert.Equal(ResultType.Error, result!.ResultStatus);
        }

        /// <summary>
        /// UpdateUserSMSNumber - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldUpdateUserSMSNumber()
        {
            Mock<IUserSMSService> smsServiceMock = new Mock<IUserSMSService>();
            smsServiceMock.Setup(s => s.UpdateUserSMS(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new UserProfileController(
                new Mock<ILogger<UserProfileController>>().Object,
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                smsServiceMock.Object);
            IActionResult actualResult = controller.UpdateUserSMSNumber(this.hdid, "250 123 456");

            Assert.True((bool)((JsonResult)actualResult).Value);
        }

        /// <summary>
        /// ValidateSms - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldValidateSms()
        {
            Mock<IUserSMSService> smsServiceMock = new Mock<IUserSMSService>();
            smsServiceMock.Setup(s => s.ValidateSMS(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new UserProfileController(
                new Mock<ILogger<UserProfileController>>().Object,
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                smsServiceMock.Object);
            IActionResult actualResult = controller.ValidateSMS(this.hdid, "205 123 4567");
            Assert.IsType<OkResult>(actualResult);
        }

        /// <summary>
        /// ValidateSms - Sms not found error.
        /// </summary>
        [Fact]
        public void ShouldValidateSmsNotFoundResult()
        {
            Mock<IUserSMSService> smsServiceMock = new Mock<IUserSMSService>();
            smsServiceMock.Setup(s => s.ValidateSMS(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            UserProfileController controller = new UserProfileController(
                new Mock<ILogger<UserProfileController>>().Object,
                new Mock<IUserProfileService>().Object,
                httpContextAccessorMock.Object,
                new Mock<IUserEmailService>().Object,
                smsServiceMock.Object);
            IActionResult actualResult = controller.ValidateSMS(this.hdid, "205 123 4567");
            Assert.IsType<NotFoundResult>(actualResult);
        }

        private static Mock<IHttpContextAccessor> CreateValidHttpContext(string token, string userId, string hdid)
        {
            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            headerDictionary.Add("referer", "http://localhost/");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("hdid", hdid),
                new Claim("auth_time", "123"),
                new Claim("access_token", token),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);
            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);
            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties.StoreTokens(new[]
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
            UserProfile userProfile = new UserProfile
            {
                HdId = this.hdid,
                AcceptedTermsOfService = true,
            };

            return new RequestResult<UserProfileModel>
            {
                ResourcePayload = UserProfileModel.CreateFromDbModel(userProfile),
                ResultStatus = resultType,
            };
        }

        private IActionResult GetUserProfile(
            RequestResult<UserProfileModel> expected,
            Dictionary<string, UserPreferenceModel>? userPreferencePayloadMock)
        {
            // Setup
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new Mock<IUserProfileService>();
            userProfileServiceMock.Setup(s => s.GetUserProfile(this.hdid, It.IsAny<DateTime>())).Returns(expected);
            userProfileServiceMock.Setup(s => s.GetActiveTermsOfService()).Returns(new RequestResult<TermsOfServiceModel>());
            userProfileServiceMock.Setup(s => s.GetUserPreferences(this.hdid)).Returns(new RequestResult<Dictionary<string, UserPreferenceModel>>() { ResourcePayload = userPreferencePayloadMock });

            Mock<IUserEmailService> emailServiceMock = new Mock<IUserEmailService>();
            Mock<IUserSMSService> smsServiceMock = new Mock<IUserSMSService>();

            UserProfileController service = new UserProfileController(
                new Mock<ILogger<UserProfileController>>().Object,
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object);
            return service.GetUserProfile(this.hdid);
        }

        private IActionResult UpdateUserPreference(UserPreferenceModel? userPref)
        {
            // Setup
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new Mock<IUserProfileService>();
            RequestResult<UserPreferenceModel> result = new RequestResult<UserPreferenceModel>()
            {
                ResourcePayload = userPref,
                ResultStatus = ResultType.Success,
            };

            userProfileServiceMock.Setup(s => s.UpdateUserPreference(userPref)).Returns(result);

            Mock<IUserEmailService> emailServiceMock = new Mock<IUserEmailService>();
            Mock<IUserSMSService> smsServiceMock = new Mock<IUserSMSService>();

            UserProfileController service = new UserProfileController(
                new Mock<ILogger<UserProfileController>>().Object,
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object);
            return service.UpdateUserPreference(this.hdid, userPref);
        }

        private IActionResult CreateUserPreference(UserPreferenceModel? userPref)
        {
            // Setup
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            Mock<IUserProfileService> userProfileServiceMock = new Mock<IUserProfileService>();
            RequestResult<UserPreferenceModel> result = new RequestResult<UserPreferenceModel>()
            {
                ResourcePayload = userPref,
                ResultStatus = ResultType.Success,
            };

            userProfileServiceMock.Setup(s => s.CreateUserPreference(userPref)).Returns(result);

            Mock<IUserEmailService> emailServiceMock = new Mock<IUserEmailService>();
            Mock<IUserSMSService> smsServiceMock = new Mock<IUserSMSService>();

            UserProfileController service = new UserProfileController(
                new Mock<ILogger<UserProfileController>>().Object,
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object);
            return service.CreateUserPreference(this.hdid, userPref);
        }
    }
}
