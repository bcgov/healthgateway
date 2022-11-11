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

namespace HealthGateway.CommonTests.Delegates
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.CDogs;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// CDogDelegate's Unit Tests.
    /// </summary>
    public class CDogsDelegateTests
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CDogsDelegateTests"/> class.
        /// </summary>
        public CDogsDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// GenerateReportAsync - Happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGenerateReport()
        {
            ReportModel expectedReportModel = new()
            {
                Data = "Test report",
                FileName = "TestReport",
            };

            RequestResult<ReportModel> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = expectedReportModel,
                TotalResultCount = 1,
            };

            string json = JsonSerializer.Serialize(requestResult);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            ICDogsDelegate cDogsDelegate = new CDogsDelegate(
                loggerFactory.CreateLogger<CDogsDelegate>(),
                httpClientService,
                this.configuration);
            CDogsRequestModel request = new()
            {
                Data = JsonDocument.Parse("{}").RootElement,
                Options = new CDogsOptionsModel
                {
                    Overwrite = true,
                    ConvertTo = "pdf",
                    ReportName = "Test Report",
                },
                Template = new CDogsTemplateModel
                {
                    Content = "Stuff",
                    FileType = "pdf",
                },
            };

            RequestResult<ReportModel> actualResult =
                await cDogsDelegate.GenerateReportAsync(request).ConfigureAwait(true);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GenerateReportAsync - handle http exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GenerateReportCatchException()
        {
            RequestResult<ReportModel> expected = new()
            {
                ResultError = new RequestResultError
                {
                    ResultMessage = $"Unable to connect to CDogs API, HTTP Error {HttpStatusCode.Forbidden}",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.CDogs),
                },
            };

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.Forbidden,
            };
            IHttpClientService httpClientService = GetHttpClientService(httpResponseMessage);

            ICDogsDelegate cDogsDelegate = new CDogsDelegate(
                loggerFactory.CreateLogger<CDogsDelegate>(),
                httpClientService,
                this.configuration);
            CDogsRequestModel request = new()
            {
                Data = JsonDocument.Parse("{}").RootElement,
                Options = new CDogsOptionsModel
                {
                    Overwrite = true,
                    ConvertTo = "pdf",
                    ReportName = "Test Report",
                },
                Template = new CDogsTemplateModel
                {
                    Content = "Stuff",
                    FileType = "pdf",
                },
            };

            RequestResult<ReportModel> actualResult =
                await cDogsDelegate.GenerateReportAsync(request).ConfigureAwait(true);

            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(expected.ResultError.ErrorCode, actualResult.ResultError?.ErrorCode);
            Assert.Equal(expected.ResultError.ResultMessage, actualResult.ResultError?.ResultMessage);
        }

        private static IHttpClientService GetHttpClientService(HttpResponseMessage httpResponseMessage)
        {
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
            return mockHttpClientService.Object;
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "CDOGS:BaseEndpoint", "https://some-test-url/" },
                { "CDOGS:DynamicServiceLookup", "true" },
            };
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }
    }
}
