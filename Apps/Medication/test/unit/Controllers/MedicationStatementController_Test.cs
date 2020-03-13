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
namespace HealthGateway.Medication.Test
{
    using HealthGateway.Common.Authorization;
    using HealthGateway.Common.Models;
    using HealthGateway.Medication.Controllers;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Moq;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Xunit;

    public class MedicationStatementController_Test
    {
        [Fact]
        public async Task ShouldGetMedicationStatements()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string userId = "1001";

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", "Bearer TestJWT");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<IMedicationStatementService> svcMock = new Mock<IMedicationStatementService>();
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthorizationService> authzMock = new Mock<IAuthorizationService>();

            svcMock.Setup(s => s.GetMedicationStatements(hdid, null)).ReturnsAsync(new HNMessage<List<MedicationStatement>>(new List<MedicationStatement>()));
            authzMock.Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), hdid, PolicyNameConstants.UserIsPatient)).ReturnsAsync(AuthorizationResult.Success);

            MedicationStatementController controller = new MedicationStatementController(authzMock.Object, svcMock.Object, httpContextAccessorMock.Object, null);

            // Act
            IActionResult actual = await controller.GetMedicationStatements(hdid);

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult jsonResult = (JsonResult)actual;

            Assert.IsType<RequestResult<List<MedicationStatement>>>(jsonResult.Value);

            RequestResult<List<MedicationStatement>> result = (RequestResult<List<MedicationStatement>>)jsonResult.Value;

            Assert.True(result.ResourcePayload.Count == 0);
        }

        [Fact]
        public async Task ShouldMapError()
        {
            // Setup
            string errorMessage = "The error message";
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string userId = "1001";

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", "Bearer TestJWT");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthorizationService> authzMock = new Mock<IAuthorizationService>();
            authzMock.Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), hdid, PolicyNameConstants.UserIsPatient)).ReturnsAsync(AuthorizationResult.Success);

            Mock<IMedicationStatementService> svcMock = new Mock<IMedicationStatementService>();
            svcMock
                .Setup(s => s.GetMedicationStatements(hdid, null))
                .ReturnsAsync(new HNMessage<List<MedicationStatement>>(new List<MedicationStatement>()) { Result = HealthGateway.Common.Constants.ResultType.Error, ResultMessage = errorMessage });

            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);
            MedicationStatementController controller = new MedicationStatementController(authzMock.Object, svcMock.Object, httpContextAccessorMock.Object, null);

            // Act
            IActionResult actual = await controller.GetMedicationStatements(hdid);

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult jsonResult = (JsonResult)actual;
            Assert.IsType<RequestResult<List<MedicationStatement>>>(jsonResult.Value);

            RequestResult<List<MedicationStatement>> requestResult = (RequestResult<List<MedicationStatement>>)jsonResult.Value;
            Assert.Null(requestResult.ResourcePayload);
            Assert.Equal(errorMessage, requestResult.ResultMessage);
        }

        [Fact]
        public async Task ShouldForbiddenMismatchPatient()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string userId = "1001";

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", "Bearer TestJWT");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<IMedicationStatementService> svcMock = new Mock<IMedicationStatementService>();
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthorizationService> authzMock = new Mock<IAuthorizationService>();

            svcMock.Setup(s => s.GetMedicationStatements(hdid, null)).ReturnsAsync(new HNMessage<List<MedicationStatement>>(new List<MedicationStatement>()));
            authzMock.Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), hdid, PolicyNameConstants.UserIsPatient)).ReturnsAsync(AuthorizationResult.Failed);

            MedicationStatementController controller = new MedicationStatementController(authzMock.Object, svcMock.Object, httpContextAccessorMock.Object, null);

            // Act
            IActionResult actual = await controller.GetMedicationStatements(hdid);

            // Verify
            Assert.IsType<ForbidResult>(actual);
            Assert.True(actual != null);
        }
    }
}
