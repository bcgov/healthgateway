// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Immunization.Test.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Immunization.Controllers;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class ImmunizationController_Test
    {
        private readonly string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private readonly string token = "Fake Access Token";
        private readonly string userId = "1001";

        private readonly RequestResult<ImmunizationResult> expectedRequestResult = new ()
        {
            ResultStatus = Common.Constants.ResultType.Success,
            TotalResultCount = 2,
            ResourcePayload = new (
                new LoadStateModel() { RefreshInProgress = false },
                new List<ImmunizationEvent>()
                {
                    new ()
                    {
                        DateOfImmunization = DateTime.Today,
                        ProviderOrClinic = "Mocked Clinic",
                        Immunization = new ImmunizationDefinition()
                        {
                            Name = "Mocked Name",
                            ImmunizationAgents = new List<ImmunizationAgent>()
                            {
                                new ()
                                {
                                    Name = "mocked agent",
                                    Code = "mocked code",
                                    LotNumber = "mocekd lot number",
                                    ProductName = "mocked product",
                                },
                            },
                        },
                    },

                    // Add a blank agent
                    new ()
                    {
                        DateOfImmunization = DateTime.Today,
                        Immunization = new ImmunizationDefinition()
                        {
                            Name = "Mocked Name",
                            ImmunizationAgents = new List<ImmunizationAgent>(),
                        },
                    },
                },
                new List<ImmunizationRecommendation>()),
        };

        [Fact]
        public async Task ShouldGetImmunizations()
        {
            ClaimsPrincipal claimsPrincipal = this.GetClaimsPrincipal();
            var httpContextAccessorMock = this.GetHttpAccessorMock(claimsPrincipal);
            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            httpContextAccessorMock
                .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = this.token },
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            Mock<IImmunizationService> svcMock = new Mock<IImmunizationService>();
            svcMock.Setup(s => s.GetImmunizations(this.token, 0)).ReturnsAsync(this.expectedRequestResult);

            ImmunizationController controller = new ImmunizationController(
                new Mock<ILogger<ImmunizationController>>().Object,
                svcMock.Object,
                httpContextAccessorMock.Object);

            // Act
            IActionResult actual = await controller.GetImmunizations(this.hdid).ConfigureAwait(true);

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult jsonResult = (JsonResult)actual;
            Assert.IsType<RequestResult<ImmunizationResult>>(jsonResult.Value);

            RequestResult<ImmunizationResult> result = (RequestResult<ImmunizationResult>)jsonResult.Value;
            Assert.Equal(Common.Constants.ResultType.Success, result.ResultStatus);
            int count = 0;
            foreach (ImmunizationEvent? immz in result.ResourcePayload!.Immunizations)
            {
                count++;
            }

            Assert.Equal(2, count);
        }

        private ClaimsPrincipal GetClaimsPrincipal()
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, this.userId),
                new Claim("hdid", this.hdid),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private Mock<IHttpContextAccessor> GetHttpAccessorMock(ClaimsPrincipal claimsPrincipal)
        {
            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", this.token);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);
            return httpContextAccessorMock;
        }
    }
}
