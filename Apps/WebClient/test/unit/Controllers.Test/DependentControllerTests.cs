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
    using DeepEqual.Syntax;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Controllers;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// DependentController's Unit Tests.
    /// </summary>
    public class DependentControllerTests
    {
        private readonly string hdid = "mockedHdId";
        private readonly string token = "Fake Access Token";
        private readonly string userId = "mockedUserId";
        private readonly string firstName = "mocked";
        private readonly string lastname = "DependentName";

        /// <summary>
        /// GetDependents - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldGetDependents()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            Mock<IDependentService> dependentServiceMock = new Mock<IDependentService>();
            IEnumerable<DependentModel> expectedDependends = GetMockDependends();
            RequestResult<IEnumerable<DependentModel>> expectedResult = new RequestResult<IEnumerable<DependentModel>>()
            {
                ResourcePayload = expectedDependends,
                ResultStatus = Common.Constants.ResultType.Success,
            };
            dependentServiceMock.Setup(s => s.GetDependents(this.hdid, 0, 500)).Returns(expectedResult);

            DependentController dependentController = new DependentController(
                new Mock<ILogger<UserProfileController>>().Object,
                dependentServiceMock.Object,
                httpContextAccessorMock.Object);
            var actualResult = dependentController.GetAll(this.hdid);

            Assert.IsType<JsonResult>(actualResult);
            Assert.True(((JsonResult)actualResult).Value?.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// AddDependent - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldAddDependent()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            Mock<IDependentService> dependentServiceMock = new Mock<IDependentService>();
            DependentModel expectedDependend = new DependentModel()
            {
                OwnerId = $"OWNER",
                DelegateId = $"DELEGATER",
                Version = 1U,
                DependentInformation = new DependentInformation()
                {
                    DateOfBirth = new DateTime(1980, 1, 1),
                    Gender = "Female",
                    FirstName = this.firstName,
                    LastName = this.lastname,
                },
            };
            RequestResult<DependentModel> expectedResult = new RequestResult<DependentModel>()
            {
                ResourcePayload = expectedDependend,
                ResultStatus = Common.Constants.ResultType.Success,
            };
            dependentServiceMock.Setup(s => s.AddDependent(this.hdid, It.IsAny<AddDependentRequest>())).Returns(expectedResult);

            DependentController dependentController = new DependentController(
                new Mock<ILogger<UserProfileController>>().Object,
                dependentServiceMock.Object,
                httpContextAccessorMock.Object);
            var actualResult = dependentController.AddDependent(new AddDependentRequest());

            Assert.True(((JsonResult)actualResult).Value!.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// DeleteDependent - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldDeleteDependent()
        {
            string delegateId = this.hdid;
            string dependentId = "123";
            DependentModel dependentModel = new DependentModel() { DelegateId = delegateId, OwnerId = dependentId };

            RequestResult<DependentModel> expectedResult = new RequestResult<DependentModel>()
            {
                ResourcePayload = dependentModel,
                ResultStatus = Common.Constants.ResultType.Success,
            };

            Mock<IDependentService> dependentServiceMock = new Mock<IDependentService>();
            dependentServiceMock.Setup(s => s.Remove(dependentModel)).Returns(expectedResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            DependentController dependentController = new DependentController(
                new Mock<ILogger<UserProfileController>>().Object,
                dependentServiceMock.Object,
                httpContextAccessorMock.Object);
            var actualResult = dependentController.Delete(delegateId, dependentId, dependentModel);

            Assert.IsType<JsonResult>(actualResult);
            Assert.True(((JsonResult)actualResult).Value!.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// DeleteDependent - BadRequest path scenario.
        /// </summary>
        [Fact]
        public void ShouldFailDeleteDependent()
        {
            string delegateId = this.hdid;
            string dependentId = "123";
            DependentModel dependentModel = new DependentModel() { DelegateId = delegateId, OwnerId = dependentId };

            RequestResult<DependentModel> expectedResult = new RequestResult<DependentModel>()
            {
                ResourcePayload = dependentModel,
                ResultStatus = Common.Constants.ResultType.Success,
            };

            Mock<IDependentService> dependentServiceMock = new Mock<IDependentService>();
            dependentServiceMock.Setup(s => s.Remove(dependentModel)).Returns(expectedResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, delegateId);

            DependentController dependentController = new DependentController(
                new Mock<ILogger<UserProfileController>>().Object,
                dependentServiceMock.Object,
                httpContextAccessorMock.Object);

            var actualResult = dependentController.Delete("anotherId", "wrongId", dependentModel);
            Assert.IsType<BadRequestResult>(actualResult);
        }

        private static IEnumerable<DependentModel> GetMockDependends()
        {
            List<DependentModel> dependentModels = new List<DependentModel>();

            for (int i = 0; i < 10; i++)
            {
                dependentModels.Add(new DependentModel()
                {
                    OwnerId = $"OWNER00{i}",
                    DelegateId = $"DELEGATER00{i}",
                    Version = (uint)i,
                    DependentInformation = new DependentInformation()
                    {
                        PHN = $"{dependentModels}-{i}",
                        DateOfBirth = new DateTime(1980 + i, 1, 1),
                        Gender = "Female",
                        FirstName = "first",
                        LastName = "last-{i}",
                    },
                });
            }

            return dependentModels;
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
    }
}
