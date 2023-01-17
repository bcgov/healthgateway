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
namespace HealthGateway.EncounterTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Encounter.Delegates;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Models.ODR;
    using HealthGateway.Encounter.Models.PHSA;
    using HealthGateway.Encounter.Services;
    using HealthGateway.EncounterTests.Utils;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// EncounterService's Unit Tests.
    /// </summary>
    public class EncounterServiceTests
    {
        private const string Hdid = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
        private const int PhsaBackOffMilliseconds = 3000;
        private const string ConfigBackOffMilliseconds = "2000";
        private const string ConfigBaseUrl = "http:localhost";
        private const string ConfigFetchSize = "25";
        private readonly string ipAddress = "127.0.0.1";

        private readonly Claim oddClaim = new()
        {
            ClaimId = 2,
            PractitionerName = "Mock Name 2",
            ServiceDate = DateTime.ParseExact("2015/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture),
        };

        private readonly RequestResult<PatientModel> patientResult = new()
        {
            ResultStatus = ResultType.Success,
            ResourcePayload = new PatientModel
            {
                PersonalHealthNumber = "912345678",
                Birthdate = DateTime.ParseExact("1983/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture),
            },
        };

        private readonly Claim sameClaim = new()
        {
            ClaimId = 1,
            PractitionerName = "Mock Name 1",
            LocationName = "Mock Name 1",
            SpecialtyDesc = "Mocked SpecialtyDesc 1",
            ServiceDate = DateTime.ParseExact("2000/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture),
            LocationAddress = new LocationAddress
            {
                Province = "BC",
                City = "Victoria",
                PostalCode = "V6Y 0C2",
                AddrLine1 = "NoWay",
                AddrLine2 = "Alt",
            },
        };

        /// <summary>
        /// GetEncounters - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateEncounters()
        {
            RequestResult<MspVisitHistoryResponse> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = new MspVisitHistoryResponse
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

            Mock<IMspVisitDelegate> mockMspDelegate = new();
            mockMspDelegate.Setup(s => s.GetMspVisitHistoryAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(delegateResult);

            Mock<IPatientService> mockPatientService = new();
            mockPatientService.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false)).ReturnsAsync(this.patientResult);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(this.GetHttpContext());

            IEncounterService service = new EncounterService(
                new Mock<ILogger<EncounterService>>().Object,
                mockHttpContextAccessor.Object,
                mockPatientService.Object,
                mockMspDelegate.Object,
                new Mock<IHospitalVisitDelegate>().Object,
                GetIConfigurationRoot(),
                MapperUtil.InitializeAutoMapper());

            RequestResult<IEnumerable<EncounterModel>> actualResult = service.GetEncounters(hdid).Result;

            Assert.True(actualResult.ResultStatus == ResultType.Success);
            Assert.Equal(2, actualResult.ResourcePayload.Count()); // should return distinct claims only.
        }

        /// <summary>
        /// GetEncounters - Empty Claims.
        /// </summary>
        [Fact]
        public void NoClaims()
        {
            RequestResult<MspVisitHistoryResponse> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = new MspVisitHistoryResponse
                {
                    Claims = null,
                },
            };

            string hdid = "MOCKHDID";

            Mock<IMspVisitDelegate> mockMspDelegate = new();
            mockMspDelegate.Setup(s => s.GetMspVisitHistoryAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            Mock<IPatientService> mockPatientService = new();
            mockPatientService.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false)).Returns(Task.FromResult(this.patientResult));

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(this.GetHttpContext());

            IEncounterService service = new EncounterService(
                new Mock<ILogger<EncounterService>>().Object,
                mockHttpContextAccessor.Object,
                mockPatientService.Object,
                mockMspDelegate.Object,
                new Mock<IHospitalVisitDelegate>().Object,
                GetIConfigurationRoot(),
                MapperUtil.InitializeAutoMapper());

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
            RequestResult<MspVisitHistoryResponse> delegateResult = new();

            string hdid = "MOCKHDID";
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            Mock<IMspVisitDelegate> mockMspDelegate = new();
            mockMspDelegate.Setup(s => s.GetMspVisitHistoryAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            RequestResult<PatientModel> errorPatientResult = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError
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
                mockMspDelegate.Object,
                new Mock<IHospitalVisitDelegate>().Object,
                GetIConfigurationRoot(),
                MapperUtil.InitializeAutoMapper());

            RequestResult<IEnumerable<EncounterModel>> actualResult = service.GetEncounters(hdid).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            errorPatientResult.ResultError.ShouldDeepEqual(actualResult.ResultError);
        }

        /// <summary>
        /// GetHospitalVisits - returns a single row.
        /// </summary>
        [Fact]
        public void ShouldGetHospitalVisits()
        {
            // Arrange
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> hospitalVisitResults = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<IEnumerable<HospitalVisit>>
                {
                    Result = new List<HospitalVisit>
                    {
                        new()
                        {
                            EncounterId = "Id",
                            AdmitDateTime = null,
                            EndDateTime = null,
                        },
                    },
                },
                TotalResultCount = 1,
            };
            IEncounterService service = GetEncounterService(hospitalVisitResults);

            // Act
            RequestResult<HospitalVisitResult> actualResult = service.GetHospitalVisits(Hdid).Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Single(actualResult.ResourcePayload!.HospitalVisits);
            Assert.True(actualResult.TotalResultCount == 1);
        }

        /// <summary>
        /// GetHospitalVisits - returns no rows.
        /// </summary>
        [Fact]
        public void GetHospitalVisitsShouldReturnNoRows()
        {
            // Arrange
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> hospitalVisitResults = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<IEnumerable<HospitalVisit>>
                {
                    Result = Enumerable.Empty<HospitalVisit>(),
                },
                TotalResultCount = 0,
            };
            IEncounterService service = GetEncounterService(hospitalVisitResults);

            // Act
            RequestResult<HospitalVisitResult> actualResult = service.GetHospitalVisits(Hdid).Result;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Empty(actualResult.ResourcePayload!.HospitalVisits);
            Assert.True(actualResult.TotalResultCount == 0);
            Assert.True(actualResult.ResourcePayload.Loaded);
            Assert.False(actualResult.ResourcePayload.Queued);
        }

        /// <summary>
        /// GetHospitalVisits - returns refresh in progress.
        /// </summary>
        [Fact]
        public void GetHospitalVisitsShouldReturnRefreshInProgress()
        {
            // Arrange
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> hospitalVisitResults = new()
            {
                ResultStatus = ResultType.ActionRequired,
                ResourcePayload = new PhsaResult<IEnumerable<HospitalVisit>>
                {
                    Result = Enumerable.Empty<HospitalVisit>(),
                    LoadState = new()
                    {
                        Queued = true,
                        BackOffMilliseconds = PhsaBackOffMilliseconds,
                        RefreshInProgress = true,
                    },
                },
            };
            IEncounterService service = GetEncounterService(hospitalVisitResults);

            // Act
            RequestResult<HospitalVisitResult> actualResult = service.GetHospitalVisits(Hdid).Result;

            // Assert
            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Empty(actualResult.ResourcePayload!.HospitalVisits);
            Assert.True(actualResult.ResourcePayload!.Queued);
            Assert.False(actualResult.ResourcePayload!.Loaded);
            Assert.True(actualResult.ResourcePayload!.RetryIn == PhsaBackOffMilliseconds);
            Assert.True(actualResult.TotalResultCount == 0);
        }

        /// <summary>
        /// GetHospitalVisits - returns error caused by delegate.
        /// </summary>
        [Fact]
        public void GetHospitalVisitsShouldReturnError()
        {
            // Arrange
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> hospitalVisitResults = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = new PhsaResult<IEnumerable<HospitalVisit>>
                {
                    Result = Enumerable.Empty<HospitalVisit>(),
                },
            };
            IEncounterService service = GetEncounterService(hospitalVisitResults);

            // Act
            RequestResult<HospitalVisitResult> actualResult = service.GetHospitalVisits(Hdid).Result;

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Empty(actualResult.ResourcePayload!.HospitalVisits);
            Assert.True(actualResult.TotalResultCount == 0);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> configuration = new()
            {
                { "PHSA:BaseUrl", ConfigBaseUrl },
                { "PHSA:FetchSize", ConfigFetchSize },
                { "PHSA: BackOffMilliseconds", ConfigBackOffMilliseconds },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configuration.ToList())
                .Build();
        }

        private static IEncounterService GetEncounterService(
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> hospitalVisitResult)
        {
            Mock<IHospitalVisitDelegate> mockHospitalVisitDelegate = new();
            mockHospitalVisitDelegate.Setup(d => d.GetHospitalVisitsAsync(It.IsAny<string>())).Returns(Task.FromResult(hospitalVisitResult));

            return new EncounterService(
                new Mock<ILogger<EncounterService>>().Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IMspVisitDelegate>().Object,
                mockHospitalVisitDelegate.Object,
                GetIConfigurationRoot(),
                MapperUtil.InitializeAutoMapper());
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
