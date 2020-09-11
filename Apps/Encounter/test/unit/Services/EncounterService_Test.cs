
using DeepEqual.Syntax;
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
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace HealthGateway.Immunization.Test.Controller
{
    public class EncounterService_Test
    {
        private readonly IConfiguration configuration;

        public EncounterService_Test()
        {
            this.configuration = GetIConfigurationRoot(string.Empty);
        }

        [Fact]
        public void ValidateImmunization()
        {

            
            ODRHistoryQuery query = new ODRHistoryQuery()
            {
                
            };
            RequestResult<MSPVisitHistoryResponse> delegateResult = new RequestResult<MSPVisitHistoryResponse>()
            {
            };


            string hdid = "MOCKHDID";
            string ipAddress = "127.0.0.1";
            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var mockMSPDelegate = new Mock<IMSPVisitDelegate>();
            mockMSPDelegate.Object.GetMSPVisitHistoryAsync(query, hdid, ipAddress);
            mockMSPDelegate.Setup(s => s.GetMSPVisitHistoryAsync(It.IsAny<ODRHistoryQuery>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(delegateResult));
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext()
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse(ipAddress),
                }
            };

            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            ITraceService traceService = new TimedTraceService(loggerFactory.CreateLogger<TimedTraceService>());
            IEncounterService service = new EncounterService(new Mock<ILogger<EncounterService>>().Object,
                                                             traceService,
                                                             mockHttpContextAccessor.Object,
                                                             null, // patient delegate
                                                             mockMSPDelegate.Object);

            var actualResult = service.GetEncounters(hdid);
            Assert.True(expectedResult.IsDeepEqual(actualResult.Result));
        }

        [Fact]
        public void ValidateImmunizationError()
        {

            var mockDelegate = new Mock<IImmunizationDelegate>();
            RequestResult<IEnumerable<ImmunizationResponse>> delegateResult = new RequestResult<IEnumerable<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ResultMessage = "Mock Error",
                    ErrorCode = "MOCK_BAD_ERROR",
                },
            };
            RequestResult<IEnumerable<ImmunizationModel>> expectedResult = new RequestResult<IEnumerable<ImmunizationModel>>()
            {
                ResultStatus = delegateResult.ResultStatus,
                ResultError = delegateResult.ResultError,
            };

            mockDelegate.Setup(s => s.GetImmunizations(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(delegateResult));
            IImmunizationService service = new ImmunizationService(new Mock<ILogger<ImmunizationService>>().Object, mockDelegate.Object);

            var actualResult = service.GetImmunizations(string.Empty, 0);
            Assert.True(expectedResult.IsDeepEqual(actualResult.Result));
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
