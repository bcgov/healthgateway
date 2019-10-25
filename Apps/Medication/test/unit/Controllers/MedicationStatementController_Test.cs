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
    using HealthGateway.Medication.Controllers;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Infrastructure;
    using Microsoft.AspNetCore.Http;
    using HealthGateway.Common.Models;
    using Moq;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Xunit;


    public class MedicationStatementController_Test
    {
        [Fact]
        public async Task ShouldGetMedicationStatemets()
        {
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string phn = "0009735353315";
            string userId = "1001";
            string ipAddress = "10.0.0.1";

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<ConnectionInfo> connectionInfoMock = new Mock<ConnectionInfo>();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", "Bearer TestJWT");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IMedicationStatementService> svcMock = new Mock<IMedicationStatementService>();
            svcMock.Setup(s => s.GetMedicationStatementsAsync(phn, userId, ipAddress)).ReturnsAsync(new HNMessage<List<MedicationStatement>>(new List<MedicationStatement>()));

            Mock<IPatientService> patientMock = new Mock<IPatientService>();
            patientMock.Setup(s => s.GetPatientPHNAsync(hdid, "Bearer TestJWT")).ReturnsAsync(phn);

            MedicationStatementController controller = new MedicationStatementController(
                svcMock.Object,
                httpContextAccessorMock.Object,
                patientService: patientMock.Object);

            RequestResult<List<MedicationStatement>> actual = await controller.GetMedicationStatements(hdid);

            Assert.True(actual.ResourcePayload.Count == 0);
        }

        [Fact]
        public async Task ShouldMapError()
        {
            // Setup
            string errorMessage = "The error message";
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string phn = "0009735353315";
            string userId = "1001";
            string ipAddress = "10.0.0.1";

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<ConnectionInfo> connectionInfoMock = new Mock<ConnectionInfo>();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", "Bearer TestJWT");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IMedicationStatementService> svcMock = new Mock<IMedicationStatementService>();
            svcMock.Setup(s => s.GetMedicationStatementsAsync(phn, userId, ipAddress)).ReturnsAsync(new HNMessage<List<MedicationStatement>>(true, errorMessage));

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IPatientService> patientMock = new Mock<IPatientService>();
            patientMock.Setup(s => s.GetPatientPHNAsync(hdid, "Bearer TestJWT")).ReturnsAsync(phn);

            MedicationStatementController controller = new MedicationStatementController(
                svcMock.Object,
                httpContextAccessorMock.Object,
                patientService: patientMock.Object);

            // Act
            RequestResult<List<MedicationStatement>> actual = await controller.GetMedicationStatements(hdid);

            // Verify
            Assert.Null(actual.ResourcePayload);
            Assert.Equal(errorMessage, actual.ErrorMessage);
        }
    }
}
