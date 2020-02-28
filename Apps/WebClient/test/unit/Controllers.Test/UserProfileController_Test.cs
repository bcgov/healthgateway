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
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authorization;
    using HealthGateway.Common.AccessManagement.Authorization;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System;
    using Microsoft.AspNetCore.Http.Headers;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Models;
    using System.Collections.Generic;

    public class UserProfileControllerTest
    {
        [Fact]
        public async Task ShouldGetUserProfile()
        {
            string hdid = "1234567890123456789012345678901234567890123456789012";
            UserProfile userProfile = new UserProfile
            {
                HdId = hdid,
                AcceptedTermsOfService = true
            };

            RequestResult<UserProfileModel> expected = new RequestResult<UserProfileModel> {
                ResourcePayload = UserProfileModel.CreateFromDbModel(userProfile),
                ResultStatus = Common.Constants.ResultType.Success
            };

            List<Claim> claimsList = new List<Claim>();
            claimsList.Add(new Claim("auth_time", "123"));

            List<ClaimsIdentity> claimsIdentityList = new List<ClaimsIdentity>();
            claimsIdentityList.Add(new ClaimsIdentity(claimsList));

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentityList);
            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext.User).Returns(claimsPrincipal);

            Mock<IAuthorizationService> authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(s => s.AuthorizeAsync(claimsPrincipal, hdid, PolicyNameConstants.UserIsPatient))
                .ReturnsAsync(AuthorizationResult.Success);

            Mock<IUserProfileService> userProfileServiceMock = new Mock<IUserProfileService>();
            userProfileServiceMock.Setup(s => s.GetUserProfile(hdid, It.IsAny<DateTime>())).Returns(expected);
            userProfileServiceMock.Setup(s => s.GetActiveTermsOfService()).Returns(new RequestResult<TermsOfServiceModel>());

            UserProfileController service = new UserProfileController(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                authorizationServiceMock.Object
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

            UserProfileController service = new UserProfileController(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                authorizationServiceMock.Object
            );
            IActionResult actualResult = await service.GetUserProfile(hdid).ConfigureAwait(true);

            Assert.IsType<ForbidResult>(actualResult);
        }

        [Fact]
        public async Task ShouldCreateUserProfile()
        {
            string hdid = "1234567890123456789012345678901234567890123456789012";
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

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();
            HttpRequest request = new DefaultHttpContext().Request;
            request.Headers["referer"] = "http://localhost/";
            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext.Request).Returns(request);
            httpContextAccessorMock.Setup(s => s.HttpContext.User).Returns(claimsPrincipal);

            Mock<IAuthorizationService> authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(s => s.AuthorizeAsync(claimsPrincipal, hdid, PolicyNameConstants.UserIsPatient))
                .ReturnsAsync(AuthorizationResult.Success);

            Mock<IUserProfileService> userProfileServiceMock = new Mock<IUserProfileService>();
            userProfileServiceMock.Setup(s => s.CreateUserProfile(createUserRequest, It.IsAny<Uri>())).Returns(expected);

            UserProfileController service = new UserProfileController(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                authorizationServiceMock.Object
            );
            IActionResult actualResult = await service.CreateUserProfile(hdid, createUserRequest).ConfigureAwait(true);

            Assert.IsType<JsonResult>(actualResult);
            Assert.True(((JsonResult)actualResult).Value.IsDeepEqual(expected)); 
        }
    }
}
