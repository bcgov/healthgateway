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
    using HealthGateway.Common.AccessManagement.Authorization;
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
    using Microsoft.Extensions.Configuration;

    public class MedicationStatementController_Test
    {

/*** TODO: Needs attention to test ODR 
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

            Mock<IMedicationStatementService> svcMock = new Mock<IMedicationStatementService>();
            svcMock
                .Setup(s => s.GetMedicationStatementsHistory(hdid, null))
                .ReturnsAsync(new RequestResult<List<MedicationStatementHistory>>()
                {
                    ResultStatus = HealthGateway.Common.Constants.ResultType.Error,
                    ResultMessage = errorMessage
                });

            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IConfigurationSection> configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(a => a.Value).Returns("ODR");
            Mock<IConfiguration> configMock = new Mock<IConfiguration>();
            configMock.Setup(s => s.GetSection(It.IsAny<string>())).Returns(configurationSection.Object);

            MedicationStatementController controller = new MedicationStatementController(configMock.Object, svcMock.Object);

            // Act
            IActionResult actual = await controller.GetMedicationStatements(hdid);

            // Verify
            Assert.IsType<JsonResult>(actual);

            JsonResult jsonResult = (JsonResult)actual;
            Assert.IsType<RequestResult<List<MedicationStatementHistory>>>(jsonResult.Value);

            RequestResult<List<MedicationStatementHistory>> requestResult = (RequestResult<List<MedicationStatementHistory>>)jsonResult.Value;
            Assert.Null(requestResult.ResourcePayload);
            Assert.Equal(errorMessage, requestResult.ResultMessage);
            
        }
**/
    }
}

