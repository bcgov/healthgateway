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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.BCMailPlus;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// VaccineProofDelegate's Unit Tests.
    /// </summary>
    public class VaccineProofDelegateTests
    {
        private const string JobId = "HLTSHC.Oct.26.2021_094235_445674736958baf638aafcc5f834e6bc";

        private readonly IConfiguration configuration;

        private readonly string qrCodeData = string.Empty;

        private readonly Address address = new()
        {
            StreetLines = ["3815 HILLSPOINT STREET"],
            City = "CHATHAM",
            Country = "CA",
            PostalCode = "V0G 8B8",
            State = "BC",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineProofDelegateTests"/> class.
        /// </summary>
        public VaccineProofDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// MailAsync - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateMailAsyncSuccess()
        {
            VaccineProofRequest request = new()
            {
                SmartHealthCardQr = this.qrCodeData,
                Status = VaccinationStatus.Partially,
            };

            RequestResult<VaccineProofResponse> expectedRequestResult = new()
            {
                PageIndex = 0,
                PageSize = null,
                ResourcePayload = new VaccineProofResponse
                {
                    Id = JobId,
                    Status = VaccineProofRequestStatus.Started,
                },
                ResultError = null,
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
            };

            BcmpJobStatusResult response = new()
            {
                Errors = string.Empty,
                JobId = JobId,
                JobStatus = BcmpJobStatus.Started,
            };

            using HttpResponseMessage httpResponseMessage = new();
            httpResponseMessage.StatusCode = HttpStatusCode.OK;
            httpResponseMessage.Content = new StringContent(JsonSerializer.Serialize(response));

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            IVaccineProofDelegate vaccineProofDelegate = new VaccineProofDelegate(
                loggerFactory.CreateLogger<VaccineProofDelegate>(),
                GetHttpClientFactoryMock(httpResponseMessage).Object,
                this.configuration);

            RequestResult<VaccineProofResponse> actualResult = await vaccineProofDelegate.MailAsync(VaccineProofTemplate.Provincial, request, this.address);

            Assert.Equal(expectedRequestResult.ResultStatus, actualResult.ResultStatus);
            Assert.Equal(expectedRequestResult.ResultError, actualResult.ResultError);
            Assert.Equal(expectedRequestResult.ResourcePayload.Id, actualResult.ResourcePayload?.Id);
            Assert.Equal(expectedRequestResult.ResourcePayload.Status, actualResult.ResourcePayload?.Status);
        }

        /// <summary>
        /// MailAsync - Error Specified in Payload.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous unit test.
        /// </returns>
        [Fact]
        public async Task ValidateMailAsyncPayloadError()
        {
            VaccineProofRequest request = new()
            {
                SmartHealthCardQr = this.qrCodeData,
                Status = VaccinationStatus.Partially,
            };

            string response = "ERROR: Undefined execution. \"handleCall_JSON\" exited unsuccessfully. Please alert the system administrator if this recurs.";

            using HttpResponseMessage httpResponseMessage = new();
            httpResponseMessage.StatusCode = HttpStatusCode.OK;
            httpResponseMessage.Content = new StringContent(response);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            IVaccineProofDelegate vaccineProofDelegate = new VaccineProofDelegate(
                loggerFactory.CreateLogger<VaccineProofDelegate>(),
                GetHttpClientFactoryMock(httpResponseMessage).Object,
                this.configuration);

            RequestResult<VaccineProofResponse> actualResult = await vaccineProofDelegate.MailAsync(VaccineProofTemplate.Provincial, request, this.address);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
            Assert.Null(actualResult.ResourcePayload);
        }

        /// <summary>
        /// MailAsync - HTTP Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateMailAsyncHttpError()
        {
            VaccineProofRequest request = new()
            {
                SmartHealthCardQr = this.qrCodeData,
                Status = VaccinationStatus.Partially,
            };

            using HttpResponseMessage httpResponseMessage = new();
            httpResponseMessage.StatusCode = HttpStatusCode.InternalServerError;
            httpResponseMessage.Content = null;

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            IVaccineProofDelegate vaccineProofDelegate = new VaccineProofDelegate(
                loggerFactory.CreateLogger<VaccineProofDelegate>(),
                GetHttpClientFactoryMock(httpResponseMessage).Object,
                this.configuration);

            RequestResult<VaccineProofResponse> actualResult = await vaccineProofDelegate.MailAsync(VaccineProofTemplate.Provincial, request, this.address);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
            Assert.Null(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GenerateAsync - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateGenerateAsyncSuccess()
        {
            VaccineProofRequest request = new()
            {
                SmartHealthCardQr = this.qrCodeData,
                Status = VaccinationStatus.Partially,
            };

            RequestResult<VaccineProofResponse> expectedRequestResult = new()
            {
                PageIndex = 0,
                PageSize = null,
                ResourcePayload = new VaccineProofResponse
                {
                    Id = JobId,
                    Status = VaccineProofRequestStatus.Started,
                },
                ResultError = null,
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
            };

            BcmpJobStatusResult response = new()
            {
                Errors = string.Empty,
                JobId = JobId,
                JobStatus = BcmpJobStatus.Started,
            };

            using HttpResponseMessage httpResponseMessage = new();
            httpResponseMessage.StatusCode = HttpStatusCode.OK;
            httpResponseMessage.Content = new StringContent(JsonSerializer.Serialize(response));

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            IVaccineProofDelegate vaccineProofDelegate = new VaccineProofDelegate(
                loggerFactory.CreateLogger<VaccineProofDelegate>(),
                GetHttpClientFactoryMock(httpResponseMessage).Object,
                this.configuration);

            RequestResult<VaccineProofResponse> actualResult = await vaccineProofDelegate.GenerateAsync(VaccineProofTemplate.Provincial, request);

            Assert.Equal(expectedRequestResult.ResultStatus, actualResult.ResultStatus);
            Assert.Equal(expectedRequestResult.ResultError, actualResult.ResultError);
            Assert.Equal(expectedRequestResult.ResourcePayload.Id, actualResult.ResourcePayload?.Id);
            Assert.Equal(expectedRequestResult.ResourcePayload.Status, actualResult.ResourcePayload?.Status);
        }

        /// <summary>
        /// GenerateAsync - Error Specified in Payload.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateGenerateAsyncPayloadError()
        {
            VaccineProofRequest request = new()
            {
                SmartHealthCardQr = this.qrCodeData,
                Status = VaccinationStatus.Partially,
            };

            string response = "ERROR: Undefined execution. \"handleCall_JSON\" exited unsuccessfully. Please alert the system administrator if this recurs.";

            using HttpResponseMessage httpResponseMessage = new();
            httpResponseMessage.StatusCode = HttpStatusCode.OK;
            httpResponseMessage.Content = new StringContent(response);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            IVaccineProofDelegate vaccineProofDelegate = new VaccineProofDelegate(
                loggerFactory.CreateLogger<VaccineProofDelegate>(),
                GetHttpClientFactoryMock(httpResponseMessage).Object,
                this.configuration);

            RequestResult<VaccineProofResponse> actualResult = await vaccineProofDelegate.GenerateAsync(VaccineProofTemplate.Provincial, request);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
            Assert.Null(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GenerateAsync - HTTP Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateGenerateAsyncHttpError()
        {
            VaccineProofRequest request = new()
            {
                SmartHealthCardQr = this.qrCodeData,
                Status = VaccinationStatus.Partially,
            };

            using HttpResponseMessage httpResponseMessage = new();
            httpResponseMessage.StatusCode = HttpStatusCode.InternalServerError;
            httpResponseMessage.Content = null;

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            IVaccineProofDelegate vaccineProofDelegate = new VaccineProofDelegate(
                loggerFactory.CreateLogger<VaccineProofDelegate>(),
                GetHttpClientFactoryMock(httpResponseMessage).Object,
                this.configuration);

            RequestResult<VaccineProofResponse> actualResult = await vaccineProofDelegate.GenerateAsync(VaccineProofTemplate.Provincial, request);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
            Assert.Null(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetAssetAsync - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateGetAssetAsyncSuccess()
        {
            Uri jobUri = new($"https://localhost/{JobId}");
            RequestResult<ReportModel> expectedRequestResult = new()
            {
                PageIndex = 0,
                PageSize = null,
                ResourcePayload = new ReportModel
                {
                    FileName = "VaccineProof.pdf",
                },
                ResultError = null,
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
            };

            byte[] fileContents = { 1 };
            using MemoryStream memoryStream = new(fileContents);
            using HttpResponseMessage httpResponseMessage = new();
            httpResponseMessage.StatusCode = HttpStatusCode.OK;
            httpResponseMessage.Content = new StreamContent(memoryStream);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            IVaccineProofDelegate vaccineProofDelegate = new VaccineProofDelegate(
                loggerFactory.CreateLogger<VaccineProofDelegate>(),
                GetHttpClientFactoryMock(httpResponseMessage).Object,
                this.configuration);

            RequestResult<ReportModel> actualResult = await vaccineProofDelegate.GetAssetAsync(jobUri);

            Assert.Equal(expectedRequestResult.ResultStatus, actualResult.ResultStatus);
            Assert.Equal(expectedRequestResult.ResultError, actualResult.ResultError);
            Assert.Equal(expectedRequestResult.ResourcePayload.FileName, actualResult.ResourcePayload?.FileName);
            Assert.NotNull(actualResult.ResourcePayload?.Data);
        }

        /// <summary>
        /// GetAssetAsync - NotFound.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateGetAssetAsyncNotFound()
        {
            Uri jobUri = new($"https://localhost/{JobId}");
            using HttpResponseMessage httpResponseMessage = new();
            httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            httpResponseMessage.Content = new StringContent(string.Empty);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            IVaccineProofDelegate vaccineProofDelegate = new VaccineProofDelegate(
                loggerFactory.CreateLogger<VaccineProofDelegate>(),
                GetHttpClientFactoryMock(httpResponseMessage).Object,
                this.configuration);

            RequestResult<ReportModel> actualResult = await vaccineProofDelegate.GetAssetAsync(jobUri);

            Assert.Equal(ResultType.ActionRequired, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
            Assert.Null(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetAssetAsync - Empty Payload.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateGetAssetAsyncEmptyPayload()
        {
            Uri jobUri = new($"https://localhost/{JobId}");
            byte[] fileContents = Array.Empty<byte>();
            using MemoryStream memoryStream = new(fileContents);
            using HttpResponseMessage httpResponseMessage = new();
            httpResponseMessage.StatusCode = HttpStatusCode.OK;
            httpResponseMessage.Content = new StreamContent(memoryStream);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            IVaccineProofDelegate vaccineProofDelegate = new VaccineProofDelegate(
                loggerFactory.CreateLogger<VaccineProofDelegate>(),
                GetHttpClientFactoryMock(httpResponseMessage).Object,
                this.configuration);

            RequestResult<ReportModel> actualResult = await vaccineProofDelegate.GetAssetAsync(jobUri);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
            Assert.Null(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetAssetAsync - HTTP Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateGetAssetAsyncHttpError()
        {
            Uri jobUri = new($"https://localhost/{JobId}");
            using HttpResponseMessage httpResponseMessage = new();
            httpResponseMessage.StatusCode = HttpStatusCode.InternalServerError;
            httpResponseMessage.Content = null;

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            IVaccineProofDelegate vaccineProofDelegate = new VaccineProofDelegate(
                loggerFactory.CreateLogger<VaccineProofDelegate>(),
                GetHttpClientFactoryMock(httpResponseMessage).Object,
                this.configuration);

            RequestResult<ReportModel> actualResult = await vaccineProofDelegate.GetAssetAsync(jobUri);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
            Assert.Null(actualResult.ResourcePayload);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "BCMailPlus:Endpoint", "https://${HOST}/${ENV}/auth=${TOKEN}/JSON/" },
                { "BCMailPlus:Host", "bcmaildirect.gov.bc.ca" },
                { "BCMailPlus:JobEnvironment", "JOB" },
                { "BCMailPlus:JobClass", "HLTH-SHC" },
                { "BCMailPlus:Token", "secret" },
                { "BCMailPlus:SchemaVersion", "HG1" },
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static Mock<IHttpClientFactory> GetHttpClientFactoryMock(HttpResponseMessage httpResponseMessage)
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
            Mock<IHttpClientFactory> mockHttpClientFactory = new();
            mockHttpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(handlerMock.Object));

            return mockHttpClientFactory;
        }
    }
}
