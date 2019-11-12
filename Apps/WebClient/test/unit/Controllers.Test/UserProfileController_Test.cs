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
    using HealthGateway.Common.Authorization;
    using System.Security.Claims;
    using System.Threading.Tasks;

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

            DBResult<UserProfile> expected = new DBResult<UserProfile> {
                Payload = userProfile,
                Status = Database.Constant.DBStatusCode.Read
            };

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext.User).Returns(claimsPrincipal);

            Mock<IAuthorizationService> authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(s => s.AuthorizeAsync(claimsPrincipal, hdid, PolicyNameConstants.UserIsPatient))
                .ReturnsAsync(AuthorizationResult.Success);

            Mock<IUserProfileService> userProfileServiceMock = new Mock<IUserProfileService>();
            userProfileServiceMock.Setup(s => s.GetUserProfile(hdid)).Returns(expected);

            UserProfileController service = new UserProfileController(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                authorizationServiceMock.Object
            );
            IActionResult actualResult = await service.GetUserProfile(hdid).ConfigureAwait(true);

            Assert.IsType<JsonResult>(actualResult);
            Assert.True(((JsonResult)actualResult).Value.IsDeepEqual(expected.Payload)); 
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

            DBResult<UserProfile> expected = new DBResult<UserProfile>
            {
                Payload = userProfile,
                Status = Database.Constant.DBStatusCode.Created
            };

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext.User).Returns(claimsPrincipal);

            Mock<IAuthorizationService> authorizationServiceMock = new Mock<IAuthorizationService>();
            authorizationServiceMock
                .Setup(s => s.AuthorizeAsync(claimsPrincipal, hdid, PolicyNameConstants.UserIsPatient))
                .ReturnsAsync(AuthorizationResult.Success);

            Mock<IUserProfileService> userProfileServiceMock = new Mock<IUserProfileService>();
            userProfileServiceMock.Setup(s => s.CreateUserProfile(userProfile)).Returns(expected);

            UserProfileController service = new UserProfileController(
                userProfileServiceMock.Object,
                httpContextAccessorMock.Object,
                authorizationServiceMock.Object
            );
            IActionResult actualResult = await service.CreateUserProfile(hdid, userProfile).ConfigureAwait(true);

            Assert.IsType<OkResult>(actualResult);
        }
    }
}
