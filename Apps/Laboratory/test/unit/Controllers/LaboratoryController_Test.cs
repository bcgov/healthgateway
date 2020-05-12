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
    using HealthGateway.Common.AccessManagement.Authorization;
    using HealthGateway.Common.Models;
    using HealthGateway.Laboratory.Controllers;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Services;
    using Moq;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Xunit;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class LaboratoryController_Test
    {
        [Fact]
        public async Task ShouldGetLaboratoryReports()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string token = "Bearer TestJWT";
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
                new Claim("access_token", token),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
            Mock<ILaboratoryService> svcMock = new Mock<ILaboratoryService>();
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthorizationService> authzMock = new Mock<IAuthorizationService>();

            svcMock.Setup(s => s.GetLaboratoryReports(token, 0)).ReturnsAsync(new RequestResult<IEnumerable<LaboratoryReport>>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 0,
                ResourcePayload = new List<LaboratoryReport>()
            });
            
            authzMock.Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), hdid, PolicyNameConstants.UserIsPatient)).ReturnsAsync(AuthorizationResult.Success);

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            LaboratoryController controller = new LaboratoryController(loggerFactory.CreateLogger<LaboratoryController>(),  svcMock.Object, httpContextAccessorMock.Object, authzMock.Object);

            // Act
            IActionResult actual = await controller.GetLaboratoryReports();

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult jsonResult = (JsonResult)actual;
            Assert.IsType<RequestResult<IEnumerable<LaboratoryReport>>>(jsonResult.Value);

            RequestResult<IEnumerable<LaboratoryReport>> result = (RequestResult<IEnumerable<LaboratoryReport>>)jsonResult.Value;
            Assert.True(result.ResultStatus == Common.Constants.ResultType.Success);
        }

        [Fact]
        public async Task ShouldMapError()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string token = "Bearer TestJWT";
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
                new Claim("access_token", token),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
            Mock<ILaboratoryService> svcMock = new Mock<ILaboratoryService>();
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthorizationService> authzMock = new Mock<IAuthorizationService>();

            svcMock.Setup(s => s.GetLaboratoryReports(token, 0)).ReturnsAsync(new RequestResult<IEnumerable<LaboratoryReport>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultMessage = "Test Error",
                TotalResultCount = 0,
            });

            authzMock.Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), hdid, PolicyNameConstants.UserIsPatient)).ReturnsAsync(AuthorizationResult.Success);

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            LaboratoryController controller = new LaboratoryController(loggerFactory.CreateLogger<LaboratoryController>(), svcMock.Object, httpContextAccessorMock.Object, authzMock.Object);

            // Act
            IActionResult actual = await controller.GetLaboratoryReports();

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult jsonResult = (JsonResult)actual;
            Assert.IsType<RequestResult<IEnumerable<LaboratoryReport>>>(jsonResult.Value);

            RequestResult<IEnumerable<LaboratoryReport>> result = (RequestResult<IEnumerable<LaboratoryReport>>)jsonResult.Value;
            Assert.True(result.ResultStatus == Common.Constants.ResultType.Error);
        }

        [Fact]
        public async Task ShouldForbiddenMismatchPatient()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string token = "Bearer TestJWT";
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
                new Claim("access_token", token),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
            Mock<ILaboratoryService> svcMock = new Mock<ILaboratoryService>();
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthorizationService> authzMock = new Mock<IAuthorizationService>();

            svcMock.Setup(s => s.GetLaboratoryReports(token, 0)).ReturnsAsync(new RequestResult<IEnumerable<LaboratoryReport>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultMessage = "Test Error",
                TotalResultCount = 0,
            });

            authzMock.Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), hdid, PolicyNameConstants.UserIsPatient)).ReturnsAsync(AuthorizationResult.Failed);

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            LaboratoryController controller = new LaboratoryController(loggerFactory.CreateLogger<LaboratoryController>(), svcMock.Object, httpContextAccessorMock.Object, authzMock.Object);

            // Act
            IActionResult actual = await controller.GetLaboratoryReports();

            // Verify
            Assert.IsType<ForbidResult>(actual);
            Assert.True(actual != null);
        }

        private static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }
    }
}
