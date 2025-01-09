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
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
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
    using PatientModel = HealthGateway.Common.Models.PatientModel;

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

        private static readonly IConfiguration Configuration = GetIConfigurationRoot();
        private static readonly IEncounterMappingService MappingService = new EncounterMappingService(MapperUtil.InitializeAutoMapper(), Configuration);
        private readonly string ipAddress = "127.0.0.1";

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
            FeeDesc = "VALID REPORT",
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

        private readonly Claim differentClaim = new()
        {
            ClaimId = 2,
            FeeDesc = "ANOTHER VALID REPORT",
            PractitionerName = "Mock Name 2",
            ServiceDate = DateTime.ParseExact("2015/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture),
        };

        /// <summary>
        /// GetEncounters - Happy Path.
        /// </summary>
        /// <param name="canAccessDataSource">The value indicates whether the health visit data source can be accessed or not.</param>
        /// <param name="enableClaimExclusions">
        /// A value indicating whether the configuration defines any fee descriptions that
        /// should be excluded from the result.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task ValidateEncounters(bool canAccessDataSource, bool enableClaimExclusions)
        {
            IList<Claim> includedClaims = [this.sameClaim, this.differentClaim, this.sameClaim];
            IList<Claim> excludedClaims = GenerateExpectedExcludedClaims();
            RequestResult<MspVisitHistoryResponse> delegateResult = new()
            {
                ResultStatus = ResultType.Success,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = new MspVisitHistoryResponse
                {
                    Claims = [..includedClaims, ..excludedClaims],
                },
            };
            string hdid = "MOCKHDID";

            Mock<IMspVisitDelegate> mockMspDelegate = new();
            mockMspDelegate.Setup(s => s.GetMspVisitHistoryAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(delegateResult);

            Mock<IPatientService> mockPatientService = new();
            mockPatientService.Setup(s => s.GetPatientAsync(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(this.patientResult);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(this.GetHttpContext());

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            IEncounterService service = new EncounterService(
                new Mock<ILogger<EncounterService>>().Object,
                mockHttpContextAccessor.Object,
                mockPatientService.Object,
                mockMspDelegate.Object,
                new Mock<IHospitalVisitDelegate>().Object,
                patientRepository.Object,
                GetIConfigurationRoot(enableClaimExclusions),
                MappingService);

            RequestResult<IEnumerable<EncounterModel>> actualResult = await service.GetEncountersAsync(hdid);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);

            if (canAccessDataSource)
            {
                // only distinct claims should be returned
                int expectedClaimCount = includedClaims.Distinct().Count();
                if (!enableClaimExclusions)
                {
                    // the excluded claims should only be returned if claim exclusions have not been enabled
                    expectedClaimCount += excludedClaims.Distinct().Count();
                }

                Assert.Equal(expectedClaimCount, actualResult.ResourcePayload.Count());
            }
            else
            {
                Assert.Empty(actualResult.ResourcePayload);
            }
        }

        /// <summary>
        /// GetEncounters - Empty Claims.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task NoClaims()
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
            mockMspDelegate.Setup(s => s.GetMspVisitHistoryAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(delegateResult);

            Mock<IPatientService> mockPatientService = new();
            mockPatientService.Setup(s => s.GetPatientAsync(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(this.patientResult);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(this.GetHttpContext());

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IEncounterService service = new EncounterService(
                new Mock<ILogger<EncounterService>>().Object,
                mockHttpContextAccessor.Object,
                mockPatientService.Object,
                mockMspDelegate.Object,
                new Mock<IHospitalVisitDelegate>().Object,
                patientRepository.Object,
                Configuration,
                MappingService);

            RequestResult<IEnumerable<EncounterModel>> actualResult = await service.GetEncountersAsync(hdid);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Empty(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetEncounters - Patient Error.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task PatientError()
        {
            RequestResult<MspVisitHistoryResponse> delegateResult = new();

            string hdid = "MOCKHDID";
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            Mock<IMspVisitDelegate> mockMspDelegate = new();
            mockMspDelegate.Setup(s => s.GetMspVisitHistoryAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(delegateResult));

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
            mockPatientService.Setup(s => s.GetPatientAsync(It.IsAny<string>(), It.IsAny<PatientIdentifierType>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(errorPatientResult);

            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(this.GetHttpContext());

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            IEncounterService service = new EncounterService(
                new Mock<ILogger<EncounterService>>().Object,
                mockHttpContextAccessor.Object,
                mockPatientService.Object,
                mockMspDelegate.Object,
                new Mock<IHospitalVisitDelegate>().Object,
                patientRepository.Object,
                Configuration,
                MappingService);

            RequestResult<IEnumerable<EncounterModel>> actualResult = await service.GetEncountersAsync(hdid);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            actualResult.ResultError.ShouldDeepEqual(errorPatientResult.ResultError);
        }

        /// <summary>
        /// GetHospitalVisits - returns a single row.
        /// </summary>
        /// <param name="canAccessDataSource">The value indicates whether the hospital visit data source can be accessed or not.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGetHospitalVisits(bool canAccessDataSource)
        {
            // Arrange
            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTime expectedAdmitDateTime = now.UtcDateTime;

            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> hospitalVisitResults = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<IEnumerable<HospitalVisit>>
                {
                    Result =
                    [
                        new()
                        {
                            EncounterId = "Id",
                            AdmitDateTime = DateTime.SpecifyKind(TimeZoneInfo.ConvertTime(now, DateFormatter.GetLocalTimeZone(Configuration)).DateTime, DateTimeKind.Unspecified),
                            EndDateTime = null,
                        },
                    ],
                },
                TotalResultCount = 1,
            };
            IEncounterService service = GetEncounterService(hospitalVisitResults, canAccessDataSource);

            // Act
            RequestResult<HospitalVisitResult> actualResult = await service.GetHospitalVisitsAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.True(actualResult.ResourcePayload.Loaded);
            Assert.False(actualResult.ResourcePayload.Queued);

            if (canAccessDataSource)
            {
                Assert.Single(actualResult.ResourcePayload!.HospitalVisits);
                Assert.Equal(1, actualResult.TotalResultCount);
                Assert.Equal(expectedAdmitDateTime, actualResult.ResourcePayload.HospitalVisits.First().AdmitDateTime);
            }
            else
            {
                Assert.Empty(actualResult.ResourcePayload!.HospitalVisits);
                Assert.Equal(0, actualResult.TotalResultCount);
            }
        }

        /// <summary>
        /// GetHospitalVisits - returns no rows.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task GetHospitalVisitsShouldReturnNoRows()
        {
            // Arrange
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> hospitalVisitResults = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PhsaResult<IEnumerable<HospitalVisit>>
                {
                    Result = [],
                },
                TotalResultCount = 0,
            };
            IEncounterService service = GetEncounterService(hospitalVisitResults);

            // Act
            RequestResult<HospitalVisitResult> actualResult = await service.GetHospitalVisitsAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Empty(actualResult.ResourcePayload!.HospitalVisits);
            Assert.Equal(0, actualResult.TotalResultCount);
            Assert.True(actualResult.ResourcePayload.Loaded);
            Assert.False(actualResult.ResourcePayload.Queued);
        }

        /// <summary>
        /// GetHospitalVisits - returns refresh in progress.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task GetHospitalVisitsShouldReturnRefreshInProgress()
        {
            // Arrange
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> hospitalVisitResults = new()
            {
                ResultStatus = ResultType.ActionRequired,
                ResourcePayload = new PhsaResult<IEnumerable<HospitalVisit>>
                {
                    Result = [],
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
            RequestResult<HospitalVisitResult> actualResult = await service.GetHospitalVisitsAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Empty(actualResult.ResourcePayload!.HospitalVisits);
            Assert.True(actualResult.ResourcePayload!.Queued);
            Assert.False(actualResult.ResourcePayload!.Loaded);
            Assert.Equal(PhsaBackOffMilliseconds, actualResult.ResourcePayload!.RetryIn);
            Assert.Equal(0, actualResult.TotalResultCount);
        }

        /// <summary>
        /// GetHospitalVisits - returns error caused by delegate.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task GetHospitalVisitsShouldReturnError()
        {
            // Arrange
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> hospitalVisitResults = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = new PhsaResult<IEnumerable<HospitalVisit>>
                {
                    Result = [],
                },
            };
            IEncounterService service = GetEncounterService(hospitalVisitResults);

            // Act
            RequestResult<HospitalVisitResult> actualResult = await service.GetHospitalVisitsAsync(Hdid);

            // Assert
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Empty(actualResult.ResourcePayload!.HospitalVisits);
            Assert.Equal(0, actualResult.TotalResultCount);
        }

        private static IList<Claim> GenerateExpectedExcludedClaims()
        {
            // full current list of fee descriptions that should be excluded
            string[] excludedFeeDescriptions =
            [
                "PRIMARY CARE PANEL REPORT",
                "LFP CLINIC DIRECT PATIENT CARE TIME",
                "LFP INDIRECT PATIENT CARE TIME",
                "LFP CLINICAL ADMIN TIME",
                "LFP LOCUM CLINIC DIRECT PATIENT CARE TIME",
                "LFP LOCUM INDIRECT PATIENT CARE TIME",
                "LFP LOCUM CLINICAL ADMINISTRATION SVCS",
                "LFP TRAVEL TIME",
                "LFP LTC/PALLIATIVE DIRECT PAT CARE TIME TIME-WEEKDAY",
                "LFP LTC/PLTV CARE DIRECT PAT CARE TIME-EVENING",
                "LFP LTC/PLTV CARE DIRECT PAT CARE TIME-WKND/STAT",
                "LFP LTC/PALLIATIVE CARE DIRECT CARE TIME-NIGHT ",
                "LFP INPATIENT DIRECT PATIENT CARE TIME-WEEKDAY",
                "LFP INPATIENT DIRECT PATIENT CARE TIME-EVENING",
                "LFP INPATIENT DIRECT PATIENT CARE TIME-WKND/STAT",
                "LFP INPATIENT DIRECT PATIENT CARE TIME-NIGHT",
                "LFP PREG/NEWBORN DIRECT PAT CARE TIME-WEEKDAY",
                "LFP PREG/NEWBORN DIRECT PAT CARE TIME-EVENING",
                "LFP PREG/NEWBORN DIRECT PAT CARE TIME-WKND/STAT",
                "LFP PREG/NEWBORN DIRECT PATIENT CARE TIME-NIGHT",
                "LFP LOCUM TRAVEL TIME",
                "LFP LOCUM LTC/PALL DIRECT PAT CARE TIME - WEEKDAY",
                "LFP LOCUM LTC/PALL DIRECT PAT CARE TIME -EVENING",
                "LFP LOCUM LTC/PALL DIRECT PAT CARE TIME WKND/STAT",
                "LFP LOCUM LTC/PALL DIRECT PAT CARE TIME - NIGHT",
                "LFP LOCUM INPATIENT DIRECT PAT CARE TIME - WEEKDAY",
                "LFP LOCUM INPATIENT DIRECT PAT CARE TIME - EVENING",
                "LFP LOCUM INPATIENT DIRECT PAT CARE TIME-WKND/STAT",
                "LFP LOCUM INPATIENT DIRECT PAT CARE TIME - NIGHT",
                "LFP LOCUM PREG/NEWBORN DIRECT CARE TIME - WKDAY",
                "LFP LOCUM PREG/NEWBORN DIRECT CARE TIME -  EVENING",
                "LFP LOCUM PREG/NEWBORN DIRECT CARE TIME -WKND/STAT",
                "LFP LOCUM PREG/NEWBORN DIRECT CARE TIME - NIGHT",
            ];

            return excludedFeeDescriptions.Select(
                    (d, i) => new Claim
                    {
                        ClaimId = 1000 + i,
                        FeeDesc = d,
                        PractitionerName = "Mock Name 3",
                        ServiceDate = new DateTime(2010, 7, 15, 0, 0, 0, DateTimeKind.Unspecified).AddDays(i),
                    })
                .ToList();
        }

        private static IConfigurationRoot GetIConfigurationRoot(bool enableClaimExclusions = true)
        {
            Dictionary<string, string?> configuration = new()
            {
                { "PHSA:BaseUrl", ConfigBaseUrl },
                { "PHSA:FetchSize", ConfigFetchSize },
                { "PHSA:BackOffMilliseconds", ConfigBackOffMilliseconds },
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
            };

            if (enableClaimExclusions)
            {
                // current list of excluded fee description prefixes from appsettings.json
                configuration["MspVisit:ExcludedFeeDescriptions"] =
                    "LFP DIRECT PATIENT CARE TIME,LFP LOCUM DIRECT PATIENT CARE TIME,PRIMARY CARE PANEL REPORT,LFP CLINIC DIRECT PATIENT CARE TIME,LFP INDIRECT PATIENT CARE TIME,LFP CLINICAL ADMIN TIME,LFP LOCUM CLINIC DIRECT PATIENT CARE TIME,LFP LOCUM INDIRECT PATIENT CARE TIME,LFP LOCUM CLINICAL ADMINISTRATION,LFP TRAVEL TIME,LFP LTC/PALLIATIVE DIRECT PAT CARE TIME TIME,LFP LTC/PLTV CARE DIRECT PAT CARE TIME,LFP LTC/PALLIATIVE CARE DIRECT CARE TIME,LFP INPATIENT DIRECT PATIENT CARE TIME,LFP PREG/NEWBORN DIRECT PAT CARE TIME,LFP PREG/NEWBORN DIRECT PATIENT CARE TIME,LFP LOCUM TRAVEL TIME,LFP LOCUM LTC/PALL DIRECT PAT CARE TIME,LFP LOCUM INPATIENT DIRECT PAT CARE TIME,LFP LOCUM PREG/NEWBORN DIRECT CARE TIME";
            }

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configuration)
                .Build();
        }

        private static IEncounterService GetEncounterService(
            RequestResult<PhsaResult<IEnumerable<HospitalVisit>>> hospitalVisitResult,
            bool canAccessDataSource = true)
        {
            Mock<IHospitalVisitDelegate> mockHospitalVisitDelegate = new();
            mockHospitalVisitDelegate.Setup(d => d.GetHospitalVisitsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(hospitalVisitResult));

            Mock<IPatientRepository> patientRepository = new();
            patientRepository.Setup(p => p.CanAccessDataSourceAsync(It.IsAny<string>(), It.IsAny<DataSource>(), It.IsAny<CancellationToken>())).ReturnsAsync(canAccessDataSource);

            return new EncounterService(
                new Mock<ILogger<EncounterService>>().Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IMspVisitDelegate>().Object,
                mockHospitalVisitDelegate.Object,
                patientRepository.Object,
                Configuration,
                MappingService);
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
            context.Request.Headers.Append("Authorization", "MockJWTHeader");
            return context;
        }
    }
}
