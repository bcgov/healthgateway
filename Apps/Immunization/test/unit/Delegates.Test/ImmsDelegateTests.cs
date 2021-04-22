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
namespace HealthGateway.Immunization.Test.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Models.PHSA;
    using HealthGateway.Immunization.Models.PHSA.Recommendation;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// RestImmunizationDelegate's Unit Tests.
    /// </summary>
    public class ImmsDelegateTests
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmsDelegateTests"/> class.
        /// </summary>
        public ImmsDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// GetImmunizations - Happy Path.
        /// </summary>
        [Fact]
        public void Ok()
        {
            ImmunizationViewResponse expectedViewResponse = new ImmunizationViewResponse()
            {
                Id = Guid.NewGuid(),
                SourceSystemId = "mockSourceSystemId",
                Name = "mockName",
                OccurrenceDateTime = DateTime.ParseExact("2020/09/10 17:16:10.809", "yyyy/MM/dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
            };

            PHSAResult<ImmunizationResponse> phsaResponse = new PHSAResult<ImmunizationResponse>()
            {
                Result = new ImmunizationResponse(
                    new List<ImmunizationViewResponse>() { expectedViewResponse },
                    new List<ImmunizationRecommendationResponse>()),
            };

            string json = JsonSerializer.Serialize(phsaResponse, null);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(loggerFactory.CreateLogger<RestImmunizationDelegate>(), httpClientService, this.configuration);
            var actualResult = immsDelegate.GetImmunizations("token", 0).Result;

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.Collection(actualResult.ResourcePayload?.Result?.ImmunizationViews, item => Assert.Equal(expectedViewResponse.Id, item.Id));
        }

        /// <summary>
        /// GetImmunizations - Bad Data.
        /// </summary>
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
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                },
            };

            using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(
                loggerFactory.CreateLogger<RestImmunizationDelegate>(),
                httpClientService,
                this.configuration);
            var actualResult = immsDelegate.GetImmunizations("token", 0).Result;

            Assert.True(expectedResult.IsDeepEqual(actualResult));
        }

        /// <summary>
        /// GetImmunizations - No Content.
        /// </summary>
        [Fact]
        public void NoContent()
        {
            int pageIndex = 0;
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NoContent,
                Content = new StringContent(string.Empty),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);
            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(loggerFactory.CreateLogger<RestImmunizationDelegate>(), httpClientService, this.configuration);
            var actualResult = immsDelegate.GetImmunizations("token", pageIndex).Result;

            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Success && actualResult?.ResourcePayload?.Result == null);
            Assert.Null(actualResult?.ResourcePayload?.Result);
        }

        /// <summary>
        /// GetImmunizations - Forbidden.
        /// </summary>
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
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                },
                PageIndex = pageIndex,
            };

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new StringContent(string.Empty),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);
            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(loggerFactory.CreateLogger<RestImmunizationDelegate>(), httpClientService, this.configuration);
            var actualResult = immsDelegate.GetImmunizations("token", pageIndex).Result;

            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// GetImmunizations - RequestTimeout.
        /// </summary>
        [Fact]
        public void RequestTimeout()
        {
            int pageIndex = 0;
            RequestResult<IEnumerable<ImmunizationResponse>> expectedResult = new RequestResult<IEnumerable<ImmunizationResponse>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ResultMessage = $"Unable to connect to Immunizations Endpoint, HTTP Error {HttpStatusCode.RequestTimeout}",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                },
                PageIndex = pageIndex,
            };

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.RequestTimeout,
                Content = new StringContent(string.Empty),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);
            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(loggerFactory.CreateLogger<RestImmunizationDelegate>(), httpClientService, this.configuration);
            var actualResult = immsDelegate.GetImmunizations("token", pageIndex).Result;

            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// GetImmunizations - HttpRequestException.
        /// </summary>
        [Fact]
        public void Exception()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .Throws<HttpRequestException>()
               .Verifiable();

            int pageIndex = 0;
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.RequestTimeout,
                Content = new StringContent(string.Empty),
            };
            IImmunizationDelegate immsDelegate = new RestImmunizationDelegate(loggerFactory.CreateLogger<RestImmunizationDelegate>(), mockHttpClientService.Object, this.configuration);
            var actualResult = immsDelegate.GetImmunizations("token", pageIndex).Result;

            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Error);
            Assert.True(actualResult?.ResultError?.ErrorCode == "testhostServer-CE-PHSA");
        }

        private static IHttpClientService GetHttpClientService(HttpResponseMessage httpResponseMessage)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpResponseMessage)
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            return mockHttpClientService.Object;
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
    }
}
