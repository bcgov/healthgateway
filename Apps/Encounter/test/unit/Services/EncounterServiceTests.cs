// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Encounter.Test.Service
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Services;
    using HealthGateway.Encounter.Delegates;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Models.ODR;
    using HealthGateway.Encounter.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// EncounterService's Unit Tests.
    /// </summary>
    public class EncounterServiceTests
    {
        private readonly string ipAddress = "127.0.0.1";
        private readonly Claim sameClaim = new()
        {
            ClaimId = 1,
            PractitionerName = "Mock Name 1",
            LocationName = "Mock Name 1",
            SpecialtyDesc = "Mocked SpecialtyDesc 1",
            ServiceDate = DateTime.ParseExact("2000/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture),
            LocationAddress = new LocationAddress()
            {
                Province = "BC",
                City = "Victoria",
                PostalCode = "V6Y 0C2",
                AddrLine1 = "NoWay",
                AddrLine2 = "Alt",
            },
        };

        private readonly Claim oddClaim = new()
        {
            ClaimId = 2,
            PractitionerName = "Mock Name 2",
            ServiceDate = DateTime.ParseExact("2015/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture),
        };

        private readonly RequestResult<PatientModel> patientResult = new()
        {
            ResultStatus = ResultType.Success,
            ResourcePayload = new PatientModel()
            {
                PersonalHealthNumber = "912345678",
                Birthdate = DateTime.ParseExact("1983/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture),
            },
        };

        /// <summary>
        /// GetEncounters - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateEncounters()
        {
            RequestResult<MSPVisitHistoryResponse> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = new MSPVisitHistoryResponse()
                {
                    Claims = new List<Claim>
                    {
                        this.sameClaim,
                        this.oddClaim,
                        this.sameClaim,
                    },
                },
            };
            string hdid = "MOCKHDID";

            Mock<IMSPVisitDelegate> mockMSPDelegate = new();
            mockMSPDelegate.Setup(s => s.GetMSPVisitHistoryAsync(It.IsAny<ODRHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(delegateResult);

            Mock<IPatientService> mockPatientService = new();
            mockPatientService.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false)).ReturnsAsync(this.patientResult);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(this.GetHttpContext());

            IEncounterService service = new EncounterService(
                new Mock<ILogger<EncounterService>>().Object,
                mockHttpContextAccessor.Object,
                mockPatientService.Object,
                mockMSPDelegate.Object);

            RequestResult<IEnumerable<EncounterModel>> actualResult = service.GetEncounters(hdid).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Success);
            Assert.Equal(2, actualResult.ResourcePayload.Count()); // should return distint claims only.
        }

        /// <summary>
        /// GetEncounters - Empty Claims.
        /// </summary>
        [Fact]
        public void NoClaims()
        {
            RequestResult<MSPVisitHistoryResponse> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = new MSPVisitHistoryResponse()
                {
                    Claims = null,
                },
            };

            string hdid = "MOCKHDID";

            Mock<IMSPVisitDelegate> mockMSPDelegate = new();
            mockMSPDelegate.Setup(s => s.GetMSPVisitHistoryAsync(It.IsAny<ODRHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            Mock<IPatientService> mockPatientService = new();
            mockPatientService.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false)).Returns(Task.FromResult(this.patientResult));

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(this.GetHttpContext());

            IEncounterService service = new EncounterService(
                new Mock<ILogger<EncounterService>>().Object,
                mockHttpContextAccessor.Object,
                mockPatientService.Object,
                mockMSPDelegate.Object);

            RequestResult<IEnumerable<EncounterModel>> actualResult = service.GetEncounters(hdid).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Success);
            Assert.False(actualResult.ResourcePayload.Any());
        }

        /// <summary>
        /// GetEncounters - Patient Error.
        /// </summary>
        [Fact]
        public void PatientError()
        {
            RequestResult<MSPVisitHistoryResponse> delegateResult = new();

            string hdid = "MOCKHDID";
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            Mock<IMSPVisitDelegate> mockMSPDelegate = new();
            mockMSPDelegate.Setup(s => s.GetMSPVisitHistoryAsync(It.IsAny<ODRHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            RequestResult<PatientModel> errorPatientResult = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ErrorCode = "Mock Error",
                    ResultMessage = "Mock Error Message",
                },
            };

            Mock<IPatientService> mockPatientService = new();
            mockPatientService.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false)).Returns(Task.FromResult(errorPatientResult));

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(this.GetHttpContext());

            IEncounterService service = new EncounterService(
                new Mock<ILogger<EncounterService>>().Object,
                mockHttpContextAccessor.Object,
                mockPatientService.Object,
                mockMSPDelegate.Object);

            RequestResult<IEnumerable<EncounterModel>> actualResult = service.GetEncounters(hdid).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Error &&
                        actualResult.ResultError.IsDeepEqual(errorPatientResult.ResultError));
        }

        private HttpContext GetHttpContext()
        {
            DefaultHttpContext context = new()
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse(this.ipAddress),
                },
            };
            context.Request.Headers.Add("Authorization", "MockJWTHeader");
            return context;
        }
    }
}
