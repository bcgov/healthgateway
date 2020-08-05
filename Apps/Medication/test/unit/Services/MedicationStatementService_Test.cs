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
    using HealthGateway.Common.Delegates;
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
    using HealthGateway.Medication.Models.ODR;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Instrumentation;

    public class MedicationService_Test
    {
        private readonly IConfiguration configuration;
        public MedicationService_Test()
        {
            this.configuration = new ConfigurationBuilder().AddJsonFile("UnitTest.json").Build();
        }


        [Fact]
        public async Task InvalidProtectiveWord()
        {
            // Setup
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

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(s => s.GetPatientPHN(hdid, "Bearer TestJWT")).Returns(
                new RequestResult<string>()
                {
                    ResourcePayload = phn,
                    ResultStatus = Common.Constants.ResultType.Success,
                });

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new Mock<IDrugLookupDelegate>();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDIN(It.IsAny<List<string>>())).Returns(new List<DrugProduct>());

            Mock<IMedStatementDelegate> medStatementDelegateMock = new Mock<IMedStatementDelegate>();
            RequestResult<MedicationHistoryResponse> requestResult = new RequestResult<MedicationHistoryResponse>();
            requestResult.ResourcePayload = new MedicationHistoryResponse();
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<MedicationHistoryQuery>(), null, It.IsAny<string>(), ipAddress)).ReturnsAsync(requestResult);

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                new Mock<ITraceService>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object);

            // Run and Verify protective word too long
            RequestResult<List<MedicationStatementHistory>> actual = await service.GetMedicationStatementsHistory(hdid, "TOOLONG4U");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "SHORT");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "SHORT|");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "SHORT~");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "SHORT^");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "SHORT\\");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "SHORT&");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "Test|");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "      ");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);

            // Run and Verify invalid char
            actual = await service.GetMedicationStatementsHistory(hdid, "Test|string");
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Protected);
        }

/** TODO: needs tweaking to work with ODR
        [Fact]
        public async Task ValidProtectiveWord()
        {
            // Setup
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string protectiveWord = "TestWord";
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

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(s => s.GetPatientPHNAsync(hdid, "Bearer TestJWT")).ReturnsAsync(phn);

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new Mock<IDrugLookupDelegate>();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDIN(It.IsAny<List<string>>())).Returns(new List<DrugProduct>());

            Mock<IMedStatementDelegate> medStatementDelegateMock = new Mock<IMedStatementDelegate>();
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<MedicationHistoryQuery>(), null, It.IsAny<string>(), ipAddress)).ReturnsAsync(new HNMessage<MedicationHistoryResponse>(new MedicationHistoryResponse()));

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                new Mock<ITraceService>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object);

            // Run and Verify
            RequestResult<List<MedicationStatementHistory>> actual = await service.GetMedicationStatementsHistory(hdid, protectiveWord);
            Assert.True(actual.ResultStatus == Common.Constants.ResultType.Success);
        }


        [Fact]
        public async Task ShouldGetMedications()
        {
            // Setup
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

            Mock<IPatientDelegate> patientDelegateMock = new Mock<IPatientDelegate>();
            patientDelegateMock.Setup(s => s.GetPatientPHNAsync(hdid, "Bearer TestJWT")).ReturnsAsync(phn);

            Mock<IDrugLookupDelegate> drugLookupDelegateMock = new Mock<IDrugLookupDelegate>();
            drugLookupDelegateMock.Setup(p => p.GetDrugProductsByDIN(It.IsAny<List<string>>())).Returns(new List<DrugProduct>());

            Mock<IMedStatementDelegate> medStatementDelegateMock = new Mock<IMedStatementDelegate>();
            medStatementDelegateMock.Setup(p => p.GetMedicationStatementsAsync(It.IsAny<MedicationHistoryQuery>(), null, It.IsAny<string>(), ipAddress)).ReturnsAsync(new HNMessage<MedicationHistoryResponse>(new MedicationHistoryResponse()));

            IMedicationStatementService service = new RestMedicationStatementService(
                new Mock<ILogger<RestMedicationStatementService>>().Object,
                new Mock<ITraceService>().Object,
                httpContextAccessorMock.Object,
                patientDelegateMock.Object,
                drugLookupDelegateMock.Object,
                medStatementDelegateMock.Object);

            // Act
            RequestResult<List<MedicationStatementHistory>> actual = await service.GetMedicationStatementsHistory(hdid, null).ConfigureAwait(true);

            // Verify
            Assert.True(actual.ResourcePayload.Count == 0);
        }**/
    }
}
