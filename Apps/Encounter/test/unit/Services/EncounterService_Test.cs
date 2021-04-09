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
    using System.Security.Cryptography;
    using System.Text;
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
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class EncounterService_Test
    {
        private readonly IConfiguration configuration;
        private readonly string ipAddress = "127.0.0.1";
        private readonly Claim sameClaim = new Claim()
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

        private readonly Claim oddClaim = new Claim()
        {
            ClaimId = 2,
            PractitionerName = "Mock Name 2",
            ServiceDate = DateTime.ParseExact("2015/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture),
        };

        private readonly RequestResult<PatientModel> patientResult = new RequestResult<PatientModel>()
        {
            ResultStatus = ResultType.Success,
            ResourcePayload = new PatientModel()
            {
                PersonalHealthNumber = "912345678",
                Birthdate = DateTime.ParseExact("1983/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture),
            },
        };

        public EncounterService_Test()
        {
            this.configuration = GetIConfigurationRoot();
        }

        [Fact]
        public void ValidateEncounters()
        {
            RequestResult<MSPVisitHistoryResponse> delegateResult = new RequestResult<MSPVisitHistoryResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = new MSPVisitHistoryResponse()
                {
                    Claims = new List<Claim>()
                    {
                        this.sameClaim,
                        this.oddClaim,
                        this.sameClaim,
                    },
                },
            };
            var expectedResult = EncounterModel.FromODRClaimModelList(delegateResult.ResourcePayload.Claims.ToList());

            string hdid = "MOCKHDID";

            var mockMSPDelegate = new Mock<IMSPVisitDelegate>();
            mockMSPDelegate.Setup(s => s.GetMSPVisitHistoryAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(delegateResult);

            var mockPatientService = new Mock<IPatientService>();
            mockPatientService.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>())).ReturnsAsync(this.patientResult);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(this.GetHttpContext());

            IEncounterService service = new EncounterService(
                new Mock<ILogger<EncounterService>>().Object,
                mockHttpContextAccessor.Object,
                mockPatientService.Object,
                mockMSPDelegate.Object);

            var actualResult = service.GetEncounters(hdid).Result;
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Success);
            Assert.Equal(2, actualResult.ResourcePayload.Count()); // should return distint claims only.
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
#pragma warning disable SCS0006 // Weak hashing function
            using var md5CryptoService = MD5.Create();
#pragma warning restore SCS0006 // Weak hashing function
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms
            var model = actualResult.ResourcePayload.First();
            var generatedId = new Guid(md5CryptoService.ComputeHash(Encoding.Default.GetBytes($"{model.EncounterDate:yyyyMMdd}{model.SpecialtyDescription}{model.PractitionerName}{model.Clinic.Name}{model.Clinic.Province}{model.Clinic.City}{model.Clinic.PostalCode}{model.Clinic.AddressLine1}{model.Clinic.AddressLine2}{model.Clinic.AddressLine3}{model.Clinic.AddressLine4}")));
            var expectedGeneratedId = new Guid(md5CryptoService.ComputeHash(Encoding.Default.GetBytes($"{this.sameClaim.ServiceDate:yyyyMMdd}{this.sameClaim.SpecialtyDesc}{this.sameClaim.PractitionerName}{this.sameClaim.LocationName}{this.sameClaim.LocationAddress.Province}{this.sameClaim.LocationAddress.City}{this.sameClaim.LocationAddress.PostalCode}{this.sameClaim.LocationAddress.AddrLine1}{this.sameClaim.LocationAddress.AddrLine2}{this.sameClaim.LocationAddress.AddrLine3}{this.sameClaim.LocationAddress.AddrLine4}")));
            Assert.Equal(expectedGeneratedId, generatedId);
        }

        [Fact]
        public void NoClaims()
        {
            RequestResult<MSPVisitHistoryResponse> delegateResult = new RequestResult<MSPVisitHistoryResponse>()
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

            var mockMSPDelegate = new Mock<IMSPVisitDelegate>();
            mockMSPDelegate.Setup(s => s.GetMSPVisitHistoryAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            RequestResult<PatientModel> patientResult = new RequestResult<PatientModel>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PatientModel()
                {
                    PersonalHealthNumber = "912345678",
                    Birthdate = DateTime.ParseExact("1983/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture),
                },
            };

            var mockPatientService = new Mock<IPatientService>();
            mockPatientService.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>())).Returns(Task.FromResult(patientResult));

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(this.GetHttpContext());

            IEncounterService service = new EncounterService(
                new Mock<ILogger<EncounterService>>().Object,
                mockHttpContextAccessor.Object,
                mockPatientService.Object,
                mockMSPDelegate.Object);

            var actualResult = service.GetEncounters(hdid).Result;
            Assert.True(actualResult.ResultStatus == ResultType.Success);
            Assert.False(actualResult.ResourcePayload.Any());
        }

        [Fact]
        public void PatientError()
        {
            RequestResult<MSPVisitHistoryResponse> delegateResult = new RequestResult<MSPVisitHistoryResponse>()
            {
            };

            string hdid = "MOCKHDID";
            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var mockMSPDelegate = new Mock<IMSPVisitDelegate>();
            mockMSPDelegate.Setup(s => s.GetMSPVisitHistoryAsync(It.IsAny<OdrHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            RequestResult<PatientModel> patientResult = new RequestResult<PatientModel>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ErrorCode = "Mock Error",
                    ResultMessage = "Mock Error Message",
                },
            };

            var mockPatientService = new Mock<IPatientService>();
            mockPatientService.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<PatientIdentifierType>())).Returns(Task.FromResult(patientResult));

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(this.GetHttpContext());

            IEncounterService service = new EncounterService(
                new Mock<ILogger<EncounterService>>().Object,
                mockHttpContextAccessor.Object,
                mockPatientService.Object,
                mockMSPDelegate.Object);

            var actualResult = service.GetEncounters(hdid).Result;
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Error &&
                        actualResult.ResultError.IsDeepEqual(patientResult.ResultError));
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()

                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }

        private HttpContext GetHttpContext()
        {
            var context = new DefaultHttpContext()
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
