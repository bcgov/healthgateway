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
    using HealthGateway.WebClient.Controllers;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;

    /// <summary>
    /// BetaRequestController's Unit Tests.
    /// </summary>
    public class BetaRequestControllerTests
    {
        private const string Hdid = "mockedHdId";
        private const string Token = "Access Token Mock";
        private const string UserId = "User ID Mock";

        /// <summary>
        /// CreateBetaRequest - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldCreateBetaRequest()
        {
            RequestResult<BetaRequest> expectedResult = new RequestResult<BetaRequest>()
            {
                ResourcePayload = new BetaRequest()
                {
                    EmailAddress = "user1@hgw.ca",
                    HdId = Hdid,
                },
                ResultStatus = Common.Constants.ResultType.Success,
            };

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(Token, UserId, Hdid);
            Mock<IBetaRequestService> betaRequestServiceMock = new Mock<IBetaRequestService>();
            betaRequestServiceMock.Setup(s => s.PutBetaRequest(It.IsAny<BetaRequest>(), It.IsAny<string>())).Returns(expectedResult);

            BetaRequestController controller = new BetaRequestController(betaRequestServiceMock.Object, httpContextAccessorMock.Object);
            var actualResult = controller.CreateBetaRequest(Hdid, expectedResult.ResourcePayload);
            Assert.True(((JsonResult)actualResult).Value.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// GetBetaRequest - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldGetBetaRequest()
        {
            BetaRequest expectedResult = new BetaRequest()
            {
                EmailAddress = "user1@hgw.ca",
                HdId = Hdid,
            };

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(Token, UserId, Hdid);
            Mock<IBetaRequestService> betaRequestServiceMock = new Mock<IBetaRequestService>();
            betaRequestServiceMock.Setup(s => s.GetBetaRequest(It.IsAny<string>())).Returns(expectedResult);

            BetaRequestController controller = new BetaRequestController(betaRequestServiceMock.Object, httpContextAccessorMock.Object);
            var actualResult = controller.GetBetaRequest(Hdid);
            Assert.True(((JsonResult)actualResult).Value.IsDeepEqual(expectedResult));
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
            authResult.Properties?.StoreTokens(new[]
            {
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = token,
                },
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
