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
        private readonly ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, UserId),
                new Claim("hdid", Hdid),
            },
            "TestAuth"));

        /// <summary>
        /// Test for GetLabOrders.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetLabOrders()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.SetupHttpContextAccessorMock();
            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            httpContextAccessorMock
                .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(this.claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties!.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = Token, },
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object!.HttpContext!, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            Mock<ILaboratoryService> svcMock = new Mock<ILaboratoryService>();
            svcMock.Setup(s => s.GetLaboratoryOrders(Token, Hdid, 0)).ReturnsAsync(new RequestResult<IEnumerable<LaboratoryModel>>()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 0,
                ResourcePayload = new List<LaboratoryModel>(),
            });
            LaboratoryController controller = new LaboratoryController(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object,
                httpContextAccessorMock.Object);

            // Act
            IActionResult actual = await controller.GetLaboratoryOrders(Hdid).ConfigureAwait(true);

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult? jsonResult = actual as JsonResult;
            Assert.IsType<RequestResult<IEnumerable<LaboratoryModel>>>(jsonResult?.Value);
            RequestResult<IEnumerable<LaboratoryModel>>? result = jsonResult?.Value as RequestResult<IEnumerable<LaboratoryModel>>;
            Assert.True(result != null && result.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// Test for GetLabOrderError.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetLabOrderError()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.SetupHttpContextAccessorMock();
            Mock<ILaboratoryService> svcMock = new Mock<ILaboratoryService>();
            svcMock.Setup(s => s.GetLaboratoryOrders(Token, Hdid, 0)).ReturnsAsync(new RequestResult<IEnumerable<LaboratoryModel>>()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = "Test Error" },
                TotalResultCount = 0,
            });

            LaboratoryController controller = new LaboratoryController(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object,
                httpContextAccessorMock.Object);

            // Act
            IActionResult actual = await controller.GetLaboratoryOrders(Hdid).ConfigureAwait(true);

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult? jsonResult = actual as JsonResult;
            RequestResult<IEnumerable<LaboratoryModel>>? result = jsonResult?.Value as RequestResult<IEnumerable<LaboratoryModel>>;
            Assert.True(result != null && result.ResultStatus == ResultType.Error);
        }

        /// <summary>
        /// Test for GetLabReport.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetLabReport()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.SetupHttpContextAccessorMock();
            Mock<ILaboratoryService> svcMock = new Mock<ILaboratoryService>();
            Guid guid = Guid.NewGuid();
            MockLaboratoryDelegate laboratoryDelegate = new MockLaboratoryDelegate();
            svcMock.Setup(s => s.GetLabReport(guid, Hdid, Token)).ReturnsAsync(await laboratoryDelegate.GetLabReport(guid, Hdid, Token).ConfigureAwait(true));

            LaboratoryController controller = new LaboratoryController(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object,
                httpContextAccessorMock.Object);

            // Act
            IActionResult actual = await controller.GetLaboratoryReport(guid, Hdid).ConfigureAwait(true);

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult? jsonResult = actual as JsonResult;
            RequestResult<LaboratoryReport>? result = jsonResult?.Value as RequestResult<LaboratoryReport>;
            Assert.True(result != null && result.ResultStatus == ResultType.Success);
        }

        /// <summary>
        /// Test for GetLabReportError.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task GetLabReportError()
        {
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.SetupHttpContextAccessorMock();
            Mock<ILaboratoryService> svcMock = new Mock<ILaboratoryService>();
            Guid guid = Guid.NewGuid();
            svcMock.Setup(s => s.GetLabReport(guid, Hdid, Token)).ReturnsAsync(new RequestResult<LaboratoryReport>()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = "Test Error" },
                TotalResultCount = 0,
            });

            LaboratoryController controller = new LaboratoryController(
                new Mock<ILogger<LaboratoryController>>().Object,
                svcMock.Object,
                httpContextAccessorMock.Object);

            // Act
            IActionResult actual = await controller.GetLaboratoryReport(guid, Hdid).ConfigureAwait(true);

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult? jsonResult = (JsonResult)actual;
            RequestResult<LaboratoryReport>? result = jsonResult?.Value as RequestResult<LaboratoryReport>;
            Assert.True(result != null && result.ResultStatus == ResultType.Error);
        }

        private Mock<IHttpContextAccessor> SetupHttpContextAccessorMock()
        {
            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", Token);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(this.claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            httpContextAccessorMock
                .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);

            var authResult = AuthenticateResult.Success(new AuthenticationTicket(this.claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
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
