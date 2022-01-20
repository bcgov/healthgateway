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
namespace HealthGateway.Encounter.Test.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Services;
    using HealthGateway.Encounter.Delegates;
    using HealthGateway.Encounter.Models.ODR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// EncounterDelegate's Unit Tests.
    /// </summary>
    public class EncounterDelegateTests
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncounterDelegateTests"/> class.
        /// </summary>
        public EncounterDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// GetMSPVisits - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetMSPVisits()
        {
            string content = @"
                   {
                        ""uuid"": ""7c51465c-7a7d-489f-b186-8755ae094d09"",
                        ""hdid"": ""P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A"",
                        ""getMspVisitHistoryResponse"": {
                            ""totalRecords"":  1,
                            ""totalPages"": 1,
                            ""claims"": [
                            {
                                ""claimId"": 1,
                                ""serviceDate"": ""2020-05-27"",
                                ""feeDesc"": ""TACROLIMUS"",
                                ""diagnosticCode"": {
                                    ""diagCode1"": ""01L"",
                                    ""diagCode2"": ""02L"",
                                    ""diagCode3"": ""03L""
                                },
                                ""specialtyDesc"": ""LABORATORY MEDICINE"",
                                ""practitionerName"": ""PRACTITIONER NAME"",
                                ""locationName"": ""PAYEE NAME"",
                                ""locationAddress"": {
                                    ""addrLine1"": ""address line 1"",
                                    ""addrLine2"": ""address line 2"",
                                    ""addrLine3"": ""address line 3"",
                                    ""addrLine4"": ""address line 4"",
                                    ""city"": ""city"",
                                    ""postalCode"": ""V9V9V9"",
                                    ""province"": ""BC""
                                }
                            }]
                        }
                    }";
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content),
            };
            Mock<HttpMessageHandler> handlerMock = new();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpResponseMessage)
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMSPVisitDelegate mspVisitDelegate = new RestMSPVisitDelegate(
                new Mock<ILogger<RestMSPVisitDelegate>>().Object,
                mockHttpClientService.Object,
                this.configuration);
            ODRHistoryQuery query = new()
            {
                PHN = "123456789",
            };

            RequestResult<MSPVisitHistoryResponse> actualResult = Task.Run(async () => await mspVisitDelegate.GetMSPVisitHistoryAsync(query, string.Empty, string.Empty).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Single(actualResult?.ResourcePayload?.Claims);
            Assert.Equal(1, actualResult?.TotalResultCount);
        }

        /// <summary>
        /// GetMSPVisits - Dynamic Lookup Error.
        /// </summary>
        [Fact]
        public void ShouldErrorDynamicLookup()
        {
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent(string.Empty),
            };
            Mock<HttpMessageHandler> handlerMock = new();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpResponseMessage)
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            IMSPVisitDelegate mspVisitDelegate = new RestMSPVisitDelegate(
                new Mock<ILogger<RestMSPVisitDelegate>>().Object,
                mockHttpClientService.Object,
                GetLocalConfig());
            ODRHistoryQuery query = new()
            {
                PHN = "123456789",
            };

            RequestResult<MSPVisitHistoryResponse> actualResult = Task.Run(async () =>
                await mspVisitDelegate.GetMSPVisitHistoryAsync(query, string.Empty, string.Empty).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// GetMSPVisits - Unauthorized.
        /// </summary>
        [Fact]
        public void ShouldErrorGetMSPVisits()
        {
            using HttpResponseMessage httpRequestMessage = new()
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent(string.Empty),
            };
            Mock<HttpMessageHandler> handlerMock = new();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpRequestMessage)
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMSPVisitDelegate mspVisitDelegate = new RestMSPVisitDelegate(
                new Mock<ILogger<RestMSPVisitDelegate>>().Object,
                mockHttpClientService.Object,
                this.configuration);
            ODRHistoryQuery query = new()
            {
                PHN = "123456789",
            };

            RequestResult<MSPVisitHistoryResponse> actualResult = Task.Run(async () => await mspVisitDelegate.GetMSPVisitHistoryAsync(query, string.Empty, string.Empty).ConfigureAwait(true)).Result;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        /// <summary>
        /// GetMSPVisits - Unknown Error.
        /// </summary>
        [Fact]
        public void ShouldException()
        {
            Mock<HttpMessageHandler> handlerMock = new();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .Throws<HttpRequestException>()
               .Verifiable();
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMSPVisitDelegate mspVisitDelegate = new RestMSPVisitDelegate(
                loggerFactory.CreateLogger<RestMSPVisitDelegate>(),
                mockHttpClientService.Object,
                this.configuration);
            ODRHistoryQuery query = new()
            {
                PHN = "123456789",
            };

            RequestResult<MSPVisitHistoryResponse> actualResult = Task.Run(async () => await mspVisitDelegate.GetMSPVisitHistoryAsync(query, string.Empty, string.Empty).ConfigureAwait(true)).Result;

            Assert.True(actualResult.ResultStatus == ResultType.Error);
            Assert.True(actualResult?.ResultError?.ErrorCode.EndsWith("-CE-ODR", StringComparison.InvariantCulture));
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }

        private static IConfigurationRoot GetLocalConfig()
        {
            Dictionary<string, string> myConfiguration = new()
            {
                { "ODR:DynamicServiceLookup", "True" },
                { "ODR:BaseEndpoint", "http://mockendpoint/" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }
    }
}
