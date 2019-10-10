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
    using Moq;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Xunit;


    public class MedicationController_Test
    {
        [Fact]
        public async Task ShouldGetMedications()
        {
            string hdid = "1192929388";
            string phn = "0009735353315";
            string userId = "1001";
            string ipAddress = "10.0.0.1";

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<ConnectionInfo> connectionInfoMock = new Mock<ConnectionInfo>();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IMedicationService> svcMock = new Mock<IMedicationService>();
            svcMock.Setup(s => s.GetMedicationsAsync(phn, userId, ipAddress)).ReturnsAsync(new HNMessage<List<MedicationStatement>>(new List<MedicationStatement>()));

            Mock<ICustomAuthorizationService> authMock = new Mock<ICustomAuthorizationService>();
            authMock.Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>(), It.IsAny<OperationAuthorizationRequirement>())).ReturnsAsync(AuthorizationResult.Success());

            Mock<IPatientService> patientMock = new Mock<IPatientService>();
            patientMock.Setup(s => s.GetPatientPHNAsync(hdid)).ReturnsAsync(phn);

            MedicationController controller = new MedicationController(
                svcMock.Object,
                httpContextAccessorMock.Object,
                authZService: authMock.Object,
                patientService: patientMock.Object);

            HNMessage<List<MedicationStatement>> actual = await controller.GetMedications(hdid); 
            
            Assert.True(actual.Message.Count == 0);
        }
    }
}
