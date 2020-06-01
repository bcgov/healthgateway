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
    using Xunit;
    using Moq;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.WebClient.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authorization;
    using HealthGateway.Common.AccessManagement.Authorization;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Models;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;

    public class UserProfileControllerTest
    {
        [Fact]
        public async Task ShouldGetUserProfile()
        {
            // Setup
            string hdid = "1234567890123456789012345678901234567890123456789012";
            string token = "Fake Access Token";
            string userId = "1001";

            UserProfile userProfile = new UserProfile
            {
                HdId = hdid,
                AcceptedTermsOfService = true
            };

            RequestResult<UserProfileModel> expected = new RequestResult<UserProfileModel>
            {
                ResourcePayload = UserProfileModel.CreateFromDbModel(userProfile),
                ResultStatus = Common.Constants.ResultType.Success
            };

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(token, userId, hdid);

            Mock<IAuthorizationService> authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(s => s.AuthorizeAsync(httpContextAccessorMock.Object.HttpContext.User, hdid, PolicyNameConstants.UserIsPatient))
                .ReturnsAsync(AuthorizationResult.Success);

            Mock<IUserProfileService> userProfileServiceMock = new Mock<IUserProfileService>();
            userProfileServiceMock.Setup(s => s.GetUserProfile(hdid, It.IsAny<DateTime>())).Returns(expected);
            userProfileServiceMock.Setup(s => s.GetActiveTermsOfService()).Returns(new RequestResult<TermsOfServiceModel>());

            Mock<IUserEmailService> emailServiceMock = new Mock<IUserEmailService>();
            Mock<IUserSMSService> smsServiceMock = new Mock<IUserSMSService>();

            UserProfileController service = new UserProfileController(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                authorizationServiceMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object
            );
            IActionResult actualResult = await service.GetUserProfile(hdid).ConfigureAwait(true);

            Assert.IsType<JsonResult>(actualResult);
            Assert.True(((JsonResult)actualResult).Value.IsDeepEqual(expected));
        }

        [Fact]
        public async Task ShouldForbidGetUserProfile()
        {
            string hdid = "1234567890123456789012345678901234567890123456789012";

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext.User).Returns(claimsPrincipal);

            Mock<IAuthorizationService> authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(s => s.AuthorizeAsync(claimsPrincipal, hdid, PolicyNameConstants.UserIsPatient))
                .ReturnsAsync(AuthorizationResult.Failed);

            Mock<IUserProfileService> userProfileServiceMock = new Mock<IUserProfileService>();
            Mock<IUserEmailService> emailServiceMock = new Mock<IUserEmailService>();
            Mock<IUserSMSService> smsServiceMock = new Mock<IUserSMSService>();

            UserProfileController service = new UserProfileController(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                authorizationServiceMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object
            );
            IActionResult actualResult = await service.GetUserProfile(hdid).ConfigureAwait(true);

            Assert.IsType<ForbidResult>(actualResult);
        }

        [Fact]
        public async Task ShouldCreateUserProfile()
        {
            string hdid = "1234567890123456789012345678901234567890123456789012";
            string token = "Fake Access Token";
            string userId = "1001";

            UserProfile userProfile = new UserProfile
            {
                HdId = hdid,
                AcceptedTermsOfService = true
            };

            CreateUserRequest createUserRequest = new CreateUserRequest
            {
                Profile = userProfile
            };

            RequestResult<UserProfileModel> expected = new RequestResult<UserProfileModel>
            {
                ResourcePayload = UserProfileModel.CreateFromDbModel(userProfile),
                ResultStatus = Common.Constants.ResultType.Success
            };

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(token, userId, hdid);

            Mock<IAuthorizationService> authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(s => s.AuthorizeAsync(httpContextAccessorMock.Object.HttpContext.User, hdid, PolicyNameConstants.UserIsPatient))
                .ReturnsAsync(AuthorizationResult.Success);

            Mock<IUserProfileService> userProfileServiceMock = new Mock<IUserProfileService>();
            userProfileServiceMock.Setup(s => s.CreateUserProfile(createUserRequest, It.IsAny<Uri>(), It.IsAny<string>())).Returns(expected);
            Mock<IUserEmailService> emailServiceMock = new Mock<IUserEmailService>();
            Mock<IUserSMSService> smsServiceMock = new Mock<IUserSMSService>();

            UserProfileController service = new UserProfileController(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                authorizationServiceMock.Object,
                emailServiceMock.Object,
                smsServiceMock.Object
            );
            IActionResult actualResult = await service.CreateUserProfile(hdid, createUserRequest).ConfigureAwait(true);

            Assert.IsType<JsonResult>(actualResult);
            Assert.True(((JsonResult)actualResult).Value.IsDeepEqual(expected));
        }

        private Mock<IHttpContextAccessor> CreateValidHttpContext(string token, string userId, string hdid)
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
                new Claim("access_token", token)
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
                new AuthenticationToken { Name = "access_token", Value = token }
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
                .ReturnsAsync(authResult);
                
            httpContextAccessorMock
                .Setup(x => x.HttpContext.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            return httpContextAccessorMock;
        }
    }
}
