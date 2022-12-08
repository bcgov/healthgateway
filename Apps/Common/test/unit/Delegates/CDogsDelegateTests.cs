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
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.CDogs;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit tests for CDogsDelegate.
    /// </summary>
    public class CDogsDelegateTests
    {
        /// <summary>
        /// GenerateReportAsync - Happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnSuccessForGenerateReportAsync()
        {
            const string reportName = "Test Report";
            const string fileType = "pdf";
            const string fileContent = "pretend this is a PDF";
            byte[] encodedFileContent = Encoding.UTF8.GetBytes(fileContent);

            RequestResult<ReportModel> expectedResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new ReportModel
                {
                    Data = Convert.ToBase64String(encodedFileContent),
                    FileName = $"{reportName}.{fileType}",
                },
                TotalResultCount = 1,
            };

            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(encodedFileContent),
            };

            Mock<ILogger<CDogsDelegate>> mockLogger = new();
            Mock<ICDogsApi> mockCdogsApi = new();
            mockCdogsApi.Setup(s => s.GenerateDocumentAsync(It.IsAny<CDogsRequestModel>())).ReturnsAsync(httpResponseMessage);

            ICDogsDelegate cdogsDelegate = new CDogsDelegate(mockLogger.Object, mockCdogsApi.Object);

            CDogsRequestModel request = GetRequestModel();
            RequestResult<ReportModel> actualResult = await cdogsDelegate.GenerateReportAsync(request).ConfigureAwait(true);

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// GenerateReportAsync - Handle HTTP errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnErrorForGenerateReportAsyncHttpError()
        {
            const HttpStatusCode statusCode = HttpStatusCode.Forbidden;

            RequestResult<ReportModel> expectedResult = new()
            {
                ResultError = new RequestResultError
                {
                    ResultMessage = $"Unable to connect to CDogs API, HTTP Error {statusCode}",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.CDogs),
                },
            };

            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = statusCode,
                Content = new ByteArrayContent(Array.Empty<byte>()),
            };

            Mock<ILogger<CDogsDelegate>> mockLogger = new();
            Mock<ICDogsApi> mockCdogsApi = new();
            mockCdogsApi.Setup(s => s.GenerateDocumentAsync(It.IsAny<CDogsRequestModel>())).ReturnsAsync(httpResponseMessage);

            ICDogsDelegate cdogsDelegate = new CDogsDelegate(mockLogger.Object, mockCdogsApi.Object);

            CDogsRequestModel request = GetRequestModel();
            RequestResult<ReportModel> actualResult = await cdogsDelegate.GenerateReportAsync(request).ConfigureAwait(true);

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// GenerateReportAsync - Handle other exceptions.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnErrorForGenerateReportAsyncException()
        {
            RequestResult<ReportModel> expectedResult = new()
            {
                ResultError = new RequestResultError
                {
                    ResultMessage = "Exception generating report",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.CDogs),
                },
            };

            Mock<ILogger<CDogsDelegate>> mockLogger = new();
            Mock<ICDogsApi> mockCdogsApi = new();
            mockCdogsApi.Setup(s => s.GenerateDocumentAsync(It.IsAny<CDogsRequestModel>())).ThrowsAsync(new InvalidOperationException());
            ICDogsDelegate cdogsDelegate = new CDogsDelegate(mockLogger.Object, mockCdogsApi.Object);

            CDogsRequestModel request = GetRequestModel();
            RequestResult<ReportModel> actualResult = await cdogsDelegate.GenerateReportAsync(request).ConfigureAwait(true);

            Assert.NotNull(actualResult.ResultError);
            Assert.Equal(expectedResult.ResultError.ErrorCode, actualResult.ResultError?.ErrorCode);
            Assert.StartsWith(expectedResult.ResultError.ResultMessage, actualResult.ResultError?.ResultMessage, StringComparison.InvariantCulture);
        }

        private static CDogsRequestModel GetRequestModel(string reportName = "Test Report", string fileType = "pdf")
        {
            return new()
            {
                Data = JsonDocument.Parse("{}").RootElement,
                Options = new CDogsOptionsModel
                {
                    Overwrite = true,
                    ConvertTo = fileType,
                    ReportName = reportName,
                },
                Template = new CDogsTemplateModel
                {
                    Content = "Stuff",
                    FileType = fileType,
                },
            };
        }
    }
}
