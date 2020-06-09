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
    using Microsoft.AspNetCore.Authentication;
    using HealthGateway.Common.Models;
    using HealthGateway.Laboratory.Controllers;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Services;
    using Moq;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Xunit;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using System;
    using HealthGateway.Laboratory.Delegates;

    public class LaboratoryController_Test
    {
        [Fact]
        public async Task GetLabOrders()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string token = "Fake Access Token";
            string userId = "1001";

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("hdid", hdid),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            httpContextAccessorMock
                .Setup(x => x.HttpContext.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = token }
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            Mock<ILaboratoryService> svcMock = new Mock<ILaboratoryService>();
            svcMock.Setup(s => s.GetLaboratoryOrders(token, 0)).ReturnsAsync(new RequestResult<IEnumerable<LaboratoryOrder>>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 0,
                ResourcePayload = new List<LaboratoryOrder>()
            });
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            LaboratoryController controller = new LaboratoryController(loggerFactory.CreateLogger<LaboratoryController>(),  svcMock.Object, httpContextAccessorMock.Object);

            // Act
            IActionResult actual = await controller.GetLaboratoryOrders();

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult jsonResult = (JsonResult)actual;
            Assert.IsType<RequestResult<IEnumerable<LaboratoryOrder>>>(jsonResult.Value);

            RequestResult<IEnumerable<LaboratoryOrder>> result = (RequestResult<IEnumerable<LaboratoryOrder>>)jsonResult.Value;
            Assert.True(result.ResultStatus == Common.Constants.ResultType.Success);
        }

        [Fact]
        public async Task GetLabOrderError()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string token = "Fake Access Token";
            string userId = "1001";

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("hdid", hdid),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            httpContextAccessorMock
                .Setup(x => x.HttpContext.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = token }
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            Mock<ILaboratoryService> svcMock = new Mock<ILaboratoryService>();
            svcMock.Setup(s => s.GetLaboratoryOrders(token, 0)).ReturnsAsync(new RequestResult<IEnumerable<LaboratoryOrder>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultMessage = "Test Error",
                TotalResultCount = 0,
            });

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            LaboratoryController controller = new LaboratoryController(loggerFactory.CreateLogger<LaboratoryController>(), svcMock.Object, httpContextAccessorMock.Object);

            // Act
            IActionResult actual = await controller.GetLaboratoryOrders();

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult jsonResult = (JsonResult)actual;
            Assert.IsType<RequestResult<IEnumerable<LaboratoryOrder>>>(jsonResult.Value);

            RequestResult<IEnumerable<LaboratoryOrder>> result = (RequestResult<IEnumerable<LaboratoryOrder>>)jsonResult.Value;
            Assert.True(result.ResultStatus == Common.Constants.ResultType.Error);
        }

        [Fact]
        public async Task GetLabReport()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string token = "Fake Access Token";
            string userId = "1001";

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("hdid", hdid),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            httpContextAccessorMock
                .Setup(x => x.HttpContext.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = token }
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            Mock<ILaboratoryService> svcMock = new Mock<ILaboratoryService>();
            Guid guid = Guid.NewGuid();
            MockLaboratoryDelegate laboratoryDelegate = new MockLaboratoryDelegate();
            svcMock.Setup(s => s.GetLabReport(guid, token)).ReturnsAsync(await laboratoryDelegate.GetLabReport(guid, token));

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            LaboratoryController controller = new LaboratoryController(loggerFactory.CreateLogger<LaboratoryController>(), svcMock.Object, httpContextAccessorMock.Object);

            // Act
            IActionResult actual = await controller.GetLaboratoryReport(guid);

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult jsonResult = (JsonResult)actual;
            Assert.IsType<RequestResult<LaboratoryReport>>(jsonResult.Value);

            RequestResult<LaboratoryReport> result = (RequestResult<LaboratoryReport>)jsonResult.Value;
            Assert.True(result.ResultStatus == Common.Constants.ResultType.Success);
        }

        [Fact]
        public async Task GetLabReportError()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string token = "Fake Access Token";
            string userId = "1001";

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("hdid", hdid),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthenticationService> authenticationMock = new Mock<IAuthenticationService>();
            httpContextAccessorMock
                .Setup(x => x.HttpContext.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationMock.Object);
            var authResult = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, JwtBearerDefaults.AuthenticationScheme));
            authResult.Properties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = token }
            });
            authenticationMock
                .Setup(x => x.AuthenticateAsync(httpContextAccessorMock.Object.HttpContext, It.IsAny<string>()))
                .ReturnsAsync(authResult);

            Mock<ILaboratoryService> svcMock = new Mock<ILaboratoryService>();
            Guid guid = Guid.NewGuid();
            svcMock.Setup(s => s.GetLabReport(guid, token)).ReturnsAsync(new RequestResult<LaboratoryReport>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultMessage = "Test Error",
                TotalResultCount = 0,
            });

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            LaboratoryController controller = new LaboratoryController(loggerFactory.CreateLogger<LaboratoryController>(), svcMock.Object, httpContextAccessorMock.Object);

            // Act
            IActionResult actual = await controller.GetLaboratoryReport(guid);

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult jsonResult = (JsonResult)actual;
            Assert.IsType<RequestResult<LaboratoryReport>>(jsonResult.Value);

            RequestResult<LaboratoryReport> result = (RequestResult<LaboratoryReport>)jsonResult.Value;
            Assert.True(result.ResultStatus == Common.Constants.ResultType.Error);
        }
    }
}
