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
namespace HealthGateway.EncounterTests
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;
    using HealthGateway.Common.Services;
    using System.Net.Http;
    using HealthGateway.Common.Models;
    using System.Threading.Tasks;
    using Moq.Protected;
    using System.Threading;
    using System.Net;
    using System.Linq;
    using System;
    using HealthGateway.Common.Instrumentation;
    using HealthGateway.Encounter.Delegates;
    using HealthGateway.Encounter.Models.ODR;
    using HealthGateway.Common.Models.ODR;

    public class EncounterDelegate_Test
    {

        public EncounterDelegate_Test()
        {
            this.configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
        }

        private readonly IConfiguration configuration;

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
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(content),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<ITraceService> mockTrace = new Mock<ITraceService>(MockBehavior.Loose);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMSPVisitDelegate mspVisitDelegate = new RestMSPVisitDelegate(
                loggerFactory.CreateLogger<RestMSPVisitDelegate>(), 
                mockTrace.Object,
                mockHttpClientService.Object, 
                this.configuration);
            ODRHistoryQuery query = new ODRHistoryQuery()
            {
                PHN = "123456789",
            };
            RequestResult<MSPVisitHistoryResponse> actualResult = Task.Run(async () => await mspVisitDelegate.GetMSPVisitHistoryAsync(query, string.Empty, string.Empty)).Result;
            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.Single(actualResult.ResourcePayload.Claims);
            Assert.Equal(1, actualResult.TotalResultCount);
        }

        [Fact]
        public void ShouldErrorGetMSPVisits()
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
                   StatusCode = HttpStatusCode.Unauthorized,
                   Content = new StringContent(string.Empty),
               })
               .Verifiable();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<ITraceService> mockTrace = new Mock<ITraceService>(MockBehavior.Loose);
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IMSPVisitDelegate mspVisitDelegate = new RestMSPVisitDelegate(
                loggerFactory.CreateLogger<RestMSPVisitDelegate>(),
                mockTrace.Object,
                mockHttpClientService.Object,
                this.configuration);
            ODRHistoryQuery query = new ODRHistoryQuery()
            {
                PHN = "123456789",
            };
            RequestResult<MSPVisitHistoryResponse> actualResult = Task.Run(async () => await mspVisitDelegate.GetMSPVisitHistoryAsync(query, string.Empty, string.Empty)).Result;
            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
        }
    }
}
