
using DeepEqual.Syntax;
using HealthGateway.Common.ErrorHandling;
using HealthGateway.Common.Instrumentation;
using HealthGateway.Common.Models;
using HealthGateway.Common.Models.PHSA;
using HealthGateway.Common.Services;
using HealthGateway.Immunization.Delegates;
using HealthGateway.Immunization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HealthGateway.Immunization.Test.Controller
{
    public class ImmsDelegate_Test
    {
        private readonly IConfiguration configuration;

        public ImmsDelegate_Test()
        {
            this.configuration = GetIConfigurationRoot(string.Empty);
        }

        [Fact]
        public void Ok()
        {
            ImmunizationResponse expectedResponse = new ImmunizationResponse()
            {
                Id = Guid.NewGuid(),
                SourceSystemId = "mockSourceSystemId",
                Name = "mockName",
                OccurrenceDateTime = DateTime.ParseExact("2020/09/10 17:16:10.809", "yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture)
            };

            PHSAResult<ImmunizationResponse> phsaResponse = new PHSAResult<ImmunizationResponse>()
            {
                Result = new List<ImmunizationResponse>()
                {
                    expectedResponse,
                }
            };

            string json = JsonSerializer.Serialize(phsaResponse, null);

            RequestResult<IEnumerable<ImmunizationResponse>> expectedResult = new RequestResult<IEnumerable<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new List<ImmunizationResponse>()
                {
                   expectedResponse,
                },
                PageIndex = 0,
                PageSize = 25,
                TotalResultCount = 1,
            };

            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ITraceService traceService = new TimedTraceService(loggerFactory.CreateLogger<TimedTraceService>());
            IHttpClientService httpClientService = GetHttpClientService(HttpStatusCode.OK, json);

            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(loggerFactory.CreateLogger<RestImmunizationDelegate>(), traceService, httpClientService, this.configuration);
            var actualResult = immsDelegate.GetImmunizations("token", 0).Result;

            Assert.True(expectedResult.IsDeepEqual(actualResult));
        }

        [Fact]
        public void BadData()
        {
            string json = "{}";

            RequestResult<IEnumerable<ImmunizationResponse>> expectedResult = new RequestResult<IEnumerable<ImmunizationResponse>>()
            {
                PageIndex = 0,
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ResultMessage = $"Error with JSON data",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA)
                },
            };

            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ITraceService traceService = new TimedTraceService(loggerFactory.CreateLogger<TimedTraceService>());
            IHttpClientService httpClientService = GetHttpClientService(HttpStatusCode.OK, json);

            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(loggerFactory.CreateLogger<RestImmunizationDelegate>(), traceService, httpClientService, this.configuration);
            var actualResult = immsDelegate.GetImmunizations("token", 0).Result;

            Assert.True(expectedResult.IsDeepEqual(actualResult));
        }

        [Fact]
        public void NoContent()
        {
            int pageIndex = 0;
            RequestResult<IEnumerable<ImmunizationResponse>> expectedResult = new RequestResult<IEnumerable<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                PageIndex = pageIndex,
            };

            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ITraceService traceService = new TimedTraceService(loggerFactory.CreateLogger<TimedTraceService>());
            IHttpClientService httpClientService = GetHttpClientService(HttpStatusCode.NoContent, string.Empty);
            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(loggerFactory.CreateLogger<RestImmunizationDelegate>(), traceService, httpClientService, this.configuration);
            var actualResult = immsDelegate.GetImmunizations("token", pageIndex).Result;

            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Success && actualResult.ResourcePayload.Count() == 0);
        }

        [Fact]
        public void Forbidden()
        {
            int pageIndex = 0;
            RequestResult<IEnumerable<ImmunizationResponse>> expectedResult = new RequestResult<IEnumerable<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {HttpStatusCode.Forbidden}",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA)
                },
                PageIndex = pageIndex,
            };

            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ITraceService traceService = new TimedTraceService(loggerFactory.CreateLogger<TimedTraceService>());
            IHttpClientService httpClientService = GetHttpClientService(HttpStatusCode.Forbidden, string.Empty);
            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(loggerFactory.CreateLogger<RestImmunizationDelegate>(), traceService, httpClientService, this.configuration);
            var actualResult = immsDelegate.GetImmunizations("token", pageIndex).Result;

            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        [Fact]
        public void Default()
        {
            int pageIndex = 0;
            RequestResult<IEnumerable<ImmunizationResponse>> expectedResult = new RequestResult<IEnumerable<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ResultMessage = $"Unable to connect to Immunizations Endpoint, HTTP Error {HttpStatusCode.RequestTimeout}",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA)
                },
                PageIndex = pageIndex,
            };

            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ITraceService traceService = new TimedTraceService(loggerFactory.CreateLogger<TimedTraceService>());
            IHttpClientService httpClientService = GetHttpClientService(HttpStatusCode.RequestTimeout, string.Empty);
            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(loggerFactory.CreateLogger<RestImmunizationDelegate>(), traceService, httpClientService, this.configuration);
            var actualResult = immsDelegate.GetImmunizations("token", pageIndex).Result;

            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }


        [Fact]
        public void Exception()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .Throws<HttpRequestException>()
               .Verifiable();

            int pageIndex = 0;
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ITraceService traceService = new TimedTraceService(loggerFactory.CreateLogger<TimedTraceService>());
            IHttpClientService httpClientService = GetHttpClientService(HttpStatusCode.RequestTimeout, string.Empty);
            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(loggerFactory.CreateLogger<RestImmunizationDelegate>(), traceService, mockHttpClientService.Object, this.configuration);
            var actualResult = immsDelegate.GetImmunizations("token", pageIndex).Result;

            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Error && actualResult.ResultError.ErrorCode == "testhostServer-CE-PHSA");
        }

        private static IHttpClientService GetHttpClientService(HttpStatusCode statusCode, string payload)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = statusCode,
                   Content = new StringContent(payload),
               })
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            return mockHttpClientService.Object;
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
