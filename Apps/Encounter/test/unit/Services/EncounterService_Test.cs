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
    using DeepEqual.Syntax;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Instrumentation;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Encounter.Delegates;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Models.ODR;
    using HealthGateway.Encounter.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Xunit;

    public class EncounterService_Test
    {
        private readonly IConfiguration configuration;

        public EncounterService_Test()
        {
            this.configuration = GetIConfigurationRoot(string.Empty);
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
                        new Claim()
                        {
                            ClaimId = 1,
                            PractitionerName = "Mock Name 1",
                            ServiceDate = DateTime.ParseExact("2000/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture)
                        },
                        new Claim()
                        {
                            ClaimId = 2,
                            PractitionerName = "Mock Name 2",
                            ServiceDate = DateTime.ParseExact("2015/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture)
                        },
                    },
                }
            };
            var expectedResult = EncounterModel.FromODRClaimModelList(delegateResult.ResourcePayload.Claims.ToList());

            string hdid = "MOCKHDID";
            string ipAddress = "127.0.0.1";
            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var mockMSPDelegate = new Mock<IMSPVisitDelegate>();
            mockMSPDelegate.Setup(s => s.GetMSPVisitHistoryAsync(It.IsAny<ODRHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            RequestResult<Patient> patientResult = new RequestResult<Patient>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new Patient()
                {
                    PersonalHealthNumber = "912345678",
                    Birthdate = DateTime.ParseExact("1983/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture)
                }
            };

            var mockPatientDelegate = new Mock<IPatientDelegate>();
            mockPatientDelegate.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<string>())).Returns(patientResult);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext()
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse(ipAddress),
                },
            };
            context.Request.Headers.Add("Authorization", "MockJWTHeader");
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);


            ITraceService traceService = new TimedTraceService(loggerFactory.CreateLogger<TimedTraceService>());
            IEncounterService service = new EncounterService(new Mock<ILogger<EncounterService>>().Object,
                                                             traceService,
                                                             mockHttpContextAccessor.Object,
                                                             mockPatientDelegate.Object,
                                                             mockMSPDelegate.Object);

            var actualResult = service.GetEncounters(hdid).Result;
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Success &&
                        actualResult.ResourcePayload.IsDeepEqual(expectedResult));
        }

        [Fact]
        public void NoClaims()
        {
            RequestResult<MSPVisitHistoryResponse> delegateResult = new RequestResult<MSPVisitHistoryResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                PageSize = 100,
                PageIndex = 1,
                ResourcePayload = new MSPVisitHistoryResponse()
                {
                    Claims = null,
                }
            };

            string hdid = "MOCKHDID";
            string ipAddress = "127.0.0.1";
            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var mockMSPDelegate = new Mock<IMSPVisitDelegate>();
            mockMSPDelegate.Setup(s => s.GetMSPVisitHistoryAsync(It.IsAny<ODRHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            RequestResult<Patient> patientResult = new RequestResult<Patient>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new Patient()
                {
                    PersonalHealthNumber = "912345678",
                    Birthdate = DateTime.ParseExact("1983/07/15", "yyyy/MM/dd", CultureInfo.InvariantCulture)
                }
            };

            var mockPatientDelegate = new Mock<IPatientDelegate>();
            mockPatientDelegate.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<string>())).Returns(patientResult);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext()
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse(ipAddress),
                },
            };
            context.Request.Headers.Add("Authorization", "MockJWTHeader");
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);


            ITraceService traceService = new TimedTraceService(loggerFactory.CreateLogger<TimedTraceService>());
            IEncounterService service = new EncounterService(new Mock<ILogger<EncounterService>>().Object,
                                                             traceService,
                                                             mockHttpContextAccessor.Object,
                                                             mockPatientDelegate.Object,
                                                             mockMSPDelegate.Object);

            var actualResult = service.GetEncounters(hdid).Result;
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Success &&
                        actualResult.ResourcePayload.Count() == 0);
        }

        [Fact]
        public void PatientError()
        {
            RequestResult<MSPVisitHistoryResponse> delegateResult = new RequestResult<MSPVisitHistoryResponse>()
            {
            };

            string hdid = "MOCKHDID";
            string ipAddress = "127.0.0.1";
            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            var mockMSPDelegate = new Mock<IMSPVisitDelegate>();
            mockMSPDelegate.Setup(s => s.GetMSPVisitHistoryAsync(It.IsAny<ODRHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));

            RequestResult<Patient> patientResult = new RequestResult<Patient>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ErrorCode = "Mock Error",
                    ResultMessage = "Mock Error Message",
                }
            };

            var mockPatientDelegate = new Mock<IPatientDelegate>();
            mockPatientDelegate.Setup(s => s.GetPatient(It.IsAny<string>(), It.IsAny<string>())).Returns(patientResult);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext()
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse(ipAddress),
                },
            };
            context.Request.Headers.Add("Authorization", "MockJWTHeader");
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            ITraceService traceService = new TimedTraceService(loggerFactory.CreateLogger<TimedTraceService>());
            IEncounterService service = new EncounterService(new Mock<ILogger<EncounterService>>().Object,
                                                             traceService,
                                                             mockHttpContextAccessor.Object,
                                                             mockPatientDelegate.Object,
                                                             mockMSPDelegate.Object);

            var actualResult = service.GetEncounters(hdid).Result;
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Error &&
                        actualResult.ResultError.IsDeepEqual(patientResult.ResultError));
        }

        [Fact]
        public void PayloadNull()
        {

        }

        private static IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }
    }
}
