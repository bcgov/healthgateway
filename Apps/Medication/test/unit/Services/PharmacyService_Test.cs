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
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Services;
    using HealthGateway.Medication.Delegates;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;
    using Microsoft.Extensions.Logging;

    public class PharmacyService_Test
    {
        private readonly IConfiguration configuration;
        public PharmacyService_Test()
        {
            this.configuration = new ConfigurationBuilder().AddJsonFile("UnitTest.json").Build();
        }

        [Fact]
        public async Task ShouldGetPharmacy()
        {
            // Setup
            string authorizationHeader = "Bearer TestJWT";
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string phn = "0009735353315";
            string userId = "1001";
            string ipAddress = "10.0.0.1";
            string pharmacyId = "BC000XX";

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);
            claimsPrincipalMock.Setup(s => s.FindFirst("hdid")).Returns(new Claim("hdid", hdid));

            Mock<ConnectionInfo> connectionInfoMock = new Mock<ConnectionInfo>();
            connectionInfoMock.Setup(s => s.RemoteIpAddress).Returns(IPAddress.Parse(ipAddress));

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", authorizationHeader);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Connection).Returns(connectionInfoMock.Object);
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(s => s.GetPatientPHNAsync(hdid, authorizationHeader)).ReturnsAsync(phn);

            Mock<IHNClientDelegate> hnClientDelegateMock = new Mock<IHNClientDelegate>();
            hnClientDelegateMock
                .Setup(s => s.GetPharmacyAsync(pharmacyId, phn, ipAddress))
                .ReturnsAsync(new HNMessage<Pharmacy>(new Pharmacy { PharmacyId = pharmacyId }));

            IPharmacyService service = new RestPharmacyService(
                new Mock<ILogger<RestPharmacyService>>().Object,
                httpContextAccessorMock.Object,
                hnClientDelegateMock.Object,
                patientDelegateMock.Object);

            // Act
            HNMessage<Pharmacy> actual = await service.GetPharmacyAsync(pharmacyId);

            // Verify
            Assert.True(actual.Message.PharmacyId == pharmacyId);
        }
    }
}
