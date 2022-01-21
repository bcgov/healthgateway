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
namespace HealthGateway.LaboratoryTests
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Laboratory.Controllers;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for LabController.
    /// </summary>
    public class LaboratoryControllerTests
    {
        // Setup
        private const string Hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
        private const string Token = "Fake Access Token";
        private const string UserId = "1001";
        private readonly ClaimsPrincipal claimsPrincipal = new(
            new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "username"),
                    new Claim(ClaimTypes.NameIdentifier, UserId),
                    new Claim("hdid", Hdid),
                },
                "TestAuth"));

        /// <summary>
        /// Test for GetCovid19Orders.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetCovid19Orders()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.SetupHttpContextAccessorMock();
            Mock<IAuthenticationService> authenticationMock = new();
            httpContextAccessorMock
                .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            AuthenticateResult authResult = AuthenticateResult.Success(new AuthenticationTicket(this.claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties!.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = Token, },
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object!.HttpContext!, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            Mock<ILaboratoryService> svcMock = new();
            svcMock.Setup(s => s.GetCovid19Orders(Hdid, 0)).ReturnsAsync(new RequestResult<IEnumerable<Covid19Order>>()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 0,
                ResourcePayload = new List<Covid19Order>(),
            });
            LaboratoryController controller = new(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object);

            // Act
            RequestResult<IEnumerable<Covid19Order>> actual = await controller.GetCovid19Orders(Hdid).ConfigureAwait(true);

            // Verify
            Assert.True(actual != null && actual.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// Test for GetLabOrderError.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetLabOrderError()
        {
            Mock<ILaboratoryService> svcMock = new();
            svcMock.Setup(s => s.GetCovid19Orders(Hdid, 0)).ReturnsAsync(new RequestResult<IEnumerable<Covid19Order>>()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = "Test Error" },
                TotalResultCount = 0,
            });

            LaboratoryController controller = new(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object);

            // Act
            RequestResult<IEnumerable<Covid19Order>> actual = await controller.GetCovid19Orders(Hdid).ConfigureAwait(true);

            // Verify
            Assert.True(actual != null && actual.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// Test for GetLabReport.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetLabReport()
        {
            Mock<ILaboratoryService> svcMock = new();
            Guid guid = Guid.NewGuid();
            MockLaboratoryDelegate laboratoryDelegate = new();
            svcMock.Setup(s => s.GetLabReport(guid, Hdid)).ReturnsAsync(await laboratoryDelegate.GetLabReport(guid, Hdid, Token).ConfigureAwait(true));

            LaboratoryController controller = new(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object);

            // Act
            RequestResult<LaboratoryReport> actual = await controller.GetLaboratoryReport(guid, Hdid).ConfigureAwait(true);

            // Verify
            Assert.True(actual != null && actual.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// Test for GetLabReportError.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetLabReportError()
        {
            Mock<ILaboratoryService> svcMock = new();
            Guid guid = Guid.NewGuid();
            svcMock.Setup(s => s.GetLabReport(guid, Hdid)).ReturnsAsync(new RequestResult<LaboratoryReport>()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = "Test Error" },
                TotalResultCount = 0,
            });

            LaboratoryController controller = new(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object);

            // Act
            RequestResult<LaboratoryReport> actual = await controller.GetLaboratoryReport(guid, Hdid).ConfigureAwait(true);

            // Verify
            Assert.True(actual != null && actual.ResultStatus == ResultType.Error);
        }

        private Mock<IHttpContextAccessor> SetupHttpContextAccessorMock()
        {
            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", Token },
            };
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.User).Returns(this.claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthenticationService> authenticationMock = new();
            httpContextAccessorMock
                .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);

            AuthenticateResult authResult = AuthenticateResult.Success(new AuthenticationTicket(this.claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties!.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = Token, },
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object!.HttpContext!, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            return httpContextAccessorMock;
        }
    }
}
