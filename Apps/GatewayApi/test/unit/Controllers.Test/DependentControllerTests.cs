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
    using System.Globalization;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.GatewayApi.Controllers;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
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
        /// GetDependentsAsync - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDependents()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            Mock<IDependentService> dependentServiceMock = new();
            IEnumerable<DependentModel> expectedDependents = GetMockDependents();
            RequestResult<IEnumerable<DependentModel>> expectedResult = new()
            {
                ResourcePayload = expectedDependents,
                ResultStatus = ResultType.Success,
            };
            dependentServiceMock.Setup(s => s.GetDependentsAsync(this.hdid, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            DependentController dependentController = new(
                dependentServiceMock.Object,
                httpContextAccessorMock.Object);
            RequestResult<IEnumerable<DependentModel>> actualResult = await dependentController.GetAll(this.hdid, It.IsAny<CancellationToken>());

            actualResult.ShouldDeepEqual(expectedResult);
        }

        /// <summary>
        /// AddDependentAsync - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldAddDependent()
        {
            DateOnly dateOfBirth = DateOnly.Parse("1980-01-01", CultureInfo.InvariantCulture);
            DateOnly expiryDate = dateOfBirth.AddYears(12);
            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);
            Mock<IDependentService> dependentServiceMock = new();
            DependentModel expectedDependent = new()
            {
                OwnerId = "OWNER",
                DelegateId = "DELEGATER",
                ExpiryDate = expiryDate,
                Version = 1U,
                DependentInformation = new DependentInformation
                {
                    DateOfBirth = dateOfBirth,
                    Gender = "Female",
                    FirstName = this.firstName,
                    LastName = this.lastname,
                },
            };
            RequestResult<DependentModel> expectedResult = new()
            {
                ResourcePayload = expectedDependent,
                ResultStatus = ResultType.Success,
            };
            dependentServiceMock.Setup(s => s.AddDependentAsync(this.hdid, It.IsAny<AddDependentRequest>(), CancellationToken.None)).ReturnsAsync(expectedResult);

            DependentController dependentController = new(
                dependentServiceMock.Object,
                httpContextAccessorMock.Object);
            RequestResult<DependentModel> actualResult = await dependentController.AddDependent(new AddDependentRequest(), CancellationToken.None);

            actualResult.ShouldDeepEqual(expectedResult);
        }

        /// <summary>
        /// DeleteDependent - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDeleteDependent()
        {
            string delegateId = this.hdid;
            string dependentId = "123";

            RequestResult<DependentModel> expectedResult = new()
            {
                ResourcePayload = new(),
                ResultStatus = ResultType.Success,
            };

            Mock<IDependentService> dependentServiceMock = new();
            dependentServiceMock.Setup(s => s.RemoveAsync(delegateId, dependentId, CancellationToken.None)).ReturnsAsync(expectedResult);

            Mock<IHttpContextAccessor> httpContextAccessorMock = CreateValidHttpContext(this.token, this.userId, this.hdid);

            DependentController dependentController = new(
                dependentServiceMock.Object,
                httpContextAccessorMock.Object);
            ActionResult<RequestResult<DependentModel>> actualResult = await dependentController.Delete(delegateId, dependentId, CancellationToken.None);
            actualResult.Value.ShouldDeepEqual(expectedResult);
        }

        private static IEnumerable<DependentModel> GetMockDependents()
        {
            List<DependentModel> dependentModels = [];

            for (int i = 0; i < 10; i++)
            {
                DateOnly dateOfBirth = DateOnly.FromDateTime(new DateTime(1980 + i, 1, 1, 0, 0, 0, DateTimeKind.Local));
                DateOnly expiryDate = dateOfBirth;

                dependentModels.Add(
                    new DependentModel
                    {
                        OwnerId = $"OWNER00{i}",
                        DelegateId = $"DELEGATER00{i}",
                        ExpiryDate = expiryDate,
                        Version = (uint)i,
                        DependentInformation = new DependentInformation
                        {
                            Phn = $"{dependentModels}-{i}",
                            DateOfBirth = dateOfBirth,
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
            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", token },
                { "referer", "http://localhost/" },
            };
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            List<Claim> claims =
            [
                new(ClaimTypes.Name, "username"),
                new(ClaimTypes.NameIdentifier, userId),
                new("hdid", hdid),
                new("auth_time", "123"),
                new("access_token", token),
            ];
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
            [
                new AuthenticationToken { Name = "access_token", Value = token },
            ]);
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
