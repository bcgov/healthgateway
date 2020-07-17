//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.CommonTests.Delegates
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;
    using HealthGateway.Common.Services;
    using System.Net.Http;
    using HealthGateway.Common.Delegates;
    using System.Threading.Tasks;
    using Moq.Protected;
    using System.Threading;
    using System.Net;
    using System.Text.Json;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using System;
    using HealthGateway.Common.Instrumentation;

    public class RestPatientDelegate_Test
    {

        private readonly IConfiguration configuration;

        public RestPatientDelegate_Test()
        {
            this.configuration = GetIConfigurationRoot();
        }

        [Fact]
        public void ShouldGetPHN()
        {
            string expectedPHN = "9735353315";
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<RestPatientDelegate> logger = loggerFactory.CreateLogger<RestPatientDelegate>();

            string json = @"{ ""hdid"":""P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A"",""personalhealthnumber"":""9735353315"",""firstname"":""BONNET"",""lastname"":""PROTERVITY"",""birthdate"":""1967-06-02T00:00:00"",""email"":""""}";
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
                   Content = new StringContent(json),
               })
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IPatientDelegate patientDelegate = new RestPatientDelegate(
                logger,
                new Mock<ITraceService>().Object,
                mockHttpClientService.Object,
                this.configuration);
            string phn = Task.Run(async () => await patientDelegate.GetPatientPHNAsync("HDID", "Bearer Token")).Result;
            Assert.True(phn == expectedPHN);
        }

        [Fact]
        public void SHouldThrowsException()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<RestPatientDelegate> logger = loggerFactory.CreateLogger<RestPatientDelegate>();

            string json = @"{ ""hdid"":""P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A"",""personalhealthnumber"":""9735353315"",""firstname"":""BONNET"",""lastname"":""PROTERVITY"",""birthdate"":""1967-06-02T00:00:00"",""email"":""""}";
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
                   Content = new StringContent(json),
               })
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            IPatientDelegate patientDelegate = new RestPatientDelegate(
                logger,
                new Mock<ITraceService>().Object,
                mockHttpClientService.Object,
                this.configuration);
            Assert.ThrowsAsync<AggregateException>(() => Task.Run(async () => await patientDelegate.GetPatientPHNAsync("HDID", "Bearer Token")));
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"PatientService:Url", "https://localhost"},
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }
    }
}
