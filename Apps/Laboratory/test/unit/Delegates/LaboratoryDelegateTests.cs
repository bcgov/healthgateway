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
namespace HealthGateway.LaboratoryTests.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Utils;
    using HealthGateway.Laboratory.Api;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using HealthGateway.LaboratoryTests.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Refit;
    using Xunit;

    /// <summary>
    /// Unit Tests for LaboratoryDelegate.
    /// </summary>
    public class LaboratoryDelegateTests
    {
        private const string AccessToken = "access_token";
        private const string Hdid = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5999999";
        private const string ReportId = "1234567890";
        private readonly IConfiguration configuration;
        private readonly IMapper autoMapper = MapperUtil.InitializeAutoMapper();

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryDelegateTests"/> class.
        /// </summary>
        public LaboratoryDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// GetCovid19Orders.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetCovid19Orders()
        {
            string expectedPhn = "9735361200";

            // Arrange
            PhsaResult<List<PhsaCovid19Order>> response = new()
            {
                Result = new()
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Phn = expectedPhn,
                    },
                },
            };

            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetCovid19OrdersAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<PhsaResult<List<PhsaCovid19Order>>> actualResult = await labDelegate.GetCovid19OrdersAsync(Hdid, AccessToken);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotEmpty(actualResult.ResourcePayload!.Result);
            Assert.Single(actualResult.ResourcePayload!.Result);
            Assert.Equal(expectedPhn, actualResult.ResourcePayload!.Result[0].Phn);
        }

        /// <summary>
        /// GetCovid19Orders handles HttpStatusCode.NoContent.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetCovid19OrderHandlesHttpStatusCodeNoContent()
        {
            // Arrange
            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            ApiException mockException = MockRefitExceptionHelper.CreateApiException(HttpStatusCode.NoContent, HttpMethod.Get);
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetCovid19OrdersAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(mockException);

            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<PhsaResult<List<PhsaCovid19Order>>> actualResult = await labDelegate.GetCovid19OrdersAsync(Hdid, AccessToken);

            // Verify
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResourcePayload.Result);
        }

        /// <summary>
        /// GetCOvid19Orders handles ProblemDetailsException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetCovid19OrdersHandlesProblemDetailsException()
        {
            string expectedMessage = $"Status: {HttpStatusCode.Unauthorized}. Error while retrieving Covid19 Orders";

            // Arrange
            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            ApiException mockException = MockRefitExceptionHelper.CreateApiException(HttpStatusCode.Unauthorized, HttpMethod.Get);
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetCovid19OrdersAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(mockException);

            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<PhsaResult<List<PhsaCovid19Order>>> actualResult = await labDelegate.GetCovid19OrdersAsync(Hdid, AccessToken);

            // Verify
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError!.ResultMessage);
        }

        /// <summary>
        /// GetCovid19Orders handles HttpRequestException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetCovid19OrdersHandleHttpRequestException()
        {
            string expectedMessage = $"Status: {HttpStatusCode.InternalServerError}. Error while retrieving Covid19 Orders";

            // Arrange
            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            HttpRequestException mockException = MockRefitExceptionHelper.CreateHttpRequestException("Internal Server Error", HttpStatusCode.InternalServerError);
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetCovid19OrdersAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(mockException);

            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<PhsaResult<List<PhsaCovid19Order>>> actualResult = await labDelegate.GetCovid19OrdersAsync(Hdid, AccessToken);

            // Verify
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError!.ResultMessage);
        }

        /// <summary>
        /// Get Covid19 Laboratory Report.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetCovid19LabReport()
        {
            string expectedPdf =
                "JVBERi0xLjcNCiW1tbW1DQoxIDAgb2JqDQo8PC9UeXBlL0NhdGFsb2cvUGFnZXMgMiAwIFIvTGFuZyhlbi1VUykgL1N0cnVjdFRyZWVSb290IDEwIDAgUi9NYXJrSW5mbzw8L01hcmtlZCB0cnVlPj4vTWV0YWRhdGEgMjAgMCBSL1ZpZXdlclByZWZlcmVuY2VzIDIxIDAgUj4+DQplbmRvYmoNCjIgMCBvYmoNCjw8L1R5cGUvUGFnZXMvQ291bnQgMS9LaWRzWyAzIDAgUl0gPj4NCmVuZG9iag0KMyAwIG9iag0KPDwvVHlwZS9QYWdlL1BhcmVudCAyIDAgUi9SZXNvdXJjZXM8PC9Gb250PDwvRjEgNSAwIFI+Pi9FeHRHU3RhdGU8PC9HUzcgNyAwIFIvR1M4IDggMCBSPj4vUHJvY1NldFsvUERGL1RleHQvSW1hZ2VCL0ltYWdlQy9JbWFnZUldID4+L01lZGlhQm94WyAwIDAgNjEyIDc5Ml0gL0NvbnRlbnRzIDQgMCBSL0dyb3VwPDwvVHlwZS9Hcm91cC9TL1RyYW5zcGFyZW5jeS9DUy9EZXZpY2VSR0I+Pi9UYWJzL1MvU3RydWN0UGFyZW50cyAwPj4NCmVuZG9iag0KNCAwIG9iag0KPDwvRmlsdGVyL0ZsYXRlRGVjb2RlL0xlbmd0aCAxOTM+Pg0Kc3RyZWFtDQp4nK2OywrCMBBF94H8w10mgmmStqaB4qKtiqLgI+BCXFSoj4Vv/x9j6UIXrnQYBi4zzDkIpkjTYJIPC8huF1mR40qJFPJVVmlIdPw0VuNWUbJs4URJ5igJ+gpKCRnBbSlR/k5CwWghdQQjrYj95ujvBguD3d3/xK5OSZMGlKyY2x/u8F2CWzbm7ZCVGx4xzHnCKr6GG1HS87QZJb9oqdAKrd+1aptG4sJDdubtmN0ef0TGiQi/EfHJQW+S4wmjfEzQDQplbmRzdHJlYW0NCmVuZG9iag0KNSAwIG9iag0KPDwvVHlwZS9Gb250L1N1YnR5cGUvVHJ1ZVR5cGUvTmFtZS9GMS9CYXNlRm9udC9CQ0RFRUUrQ2FsaWJyaS9FbmNvZGluZy9XaW5BbnNpRW5jb2RpbmcvRm9udERlc2NyaXB0b3IgNiAwIFIvRmlyc3RDaGFyIDMyL0xhc3RDaGFyIDExNi9XaWR0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czpkYz0iaHR0cDovL3B1cmwub3JnL2RjL2VsZW1lbnRzLzEuMS8iPgo8ZGM6Y3JlYXRvcj48cmRmOlNlcT48cmRmOmxpPlN0ZXBoZW4gTGF3czwvcmRmOmxpPjwvcmRmOlNlcT48L2RjOmNyZWF0b3I+PC9yZGY6RGVzY3JpcHRpb24+CjxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiICB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iPgo8eG1wOkNyZWF0b3JUb29sPk1pY3Jvc29mdMKuIFdvcmQgZm9yIE1pY3Jvc29mdCAzNjU8L3htcDpDcmVhdG9yVG9vbD48eG1wOkNyZWF0ZURhdGU+MjAyMC0wNS0xMVQxODoxMDo0Ni0wNzowMDwveG1wOkNyZWF0ZURhdGU+PHhtcDpNb2RpZnlEYXRlPjIwMjAtMDUtMTFUMTg6MTA6NDYtMDc6MDA8L3htcDpNb2RpZnlEYXRlPjwvcmRmOkRlc2NyaXB0aW9uPgo8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiAgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iPgo8eG1wTU06RG9jdW1lbnRJRD51dWlkOkQ2MzE3QUNFLTkyREMtNDlBMi1CNzY5LUYwM0FDMDQ5OTYwQjwveG1wTU06RG9jdW1lbnRJRD48eG1wTU06SW5zdGFuY2VJRD51dWlkOkQ2MzE3QUNFLTkyREMtNDlBMi1CNzY5LUYwM0FDMDQ5OTYwQjwveG1wTU06SW5zdGFuY2VJRD48L3JkZjpEZXNjcmlwdGlvbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCjwvcmRmOlJERj48L3g6eG1wbWV0YT48P3hwYWNrZXQgZW5kPSJ3Ij8+DQplbmRzdHJlYW0NCmVuZG9iag0KMjEgMCBvYmoNCjw8L0Rpc3BsYXlEb2NUaXRsZSB0cnVlPj4NCmVuZG9iag0KMjIgMCBvYmoNCjw8L1R5cGUvWFJlZi9TaXplIDIyL1dbIDEgNCAyXSAvUm9vdCAxIDAgUi9JbmZvIDkgMCBSL0lEWzxDRTdBMzFENkRDOTJBMjQ5Qjc2OUYwM0FDMDQ5OTYwQj48Q0U3QTMxRDZEQzkyQTI0OUI3NjlGMDNBQzA0OTk2MEI+XSAvRmlsdGVyL0ZsYXRlRGVjb2RlL0xlbmd0aCA4NT4+DQpzdHJlYW0NCnicY2AAgv//GYGkIAMDiFoGoe6BKcZnYIrpI5hingmmWDog1F4I9QkoD9bOBKGYIRQLhGKFUIwQCqqSDaiPjR2snX0ymOIoAFNFUWCqDmg0ADXHDBsNCmVuZHN0cmVhbQ0KZW5kb2JqDQp4cmVmDQowIDIzDQowMDAwMDAwMDEwIDY1NTM1IGYNCjAwMDAwMDAwMTcgMDAwMDAgbg0KMDAwMDAwMDE2NiAwMDAwMCBuDQowMDAwMDAwMjIyIDAwMDAwIG4NCjAwMDAwMDA0ODYgMDAwMDAgbg0KMDAwMDAwMDc1MyAwMDAwMCBuDQowMDAwMDAwOTIxIDAwMDAwIG4NCjAwMDAwMDExNjAgMDAwMDAgbg0KMDAwMDAwMTIxMyAwMDAwMCBuDQowMDAwMDAxMjY2IDAwMDAwIG4NCjAwMDAwMDAwMTEgNjU1MzUgZg0KMDAwMDAwMDAxMiA2NTUzNSBmDQowMDAwMDAwMDEzIDY1NTM1IGYNCjAwMDAwMDAwMTQgNjU1MzUgZg0KMDAwMDAwMDAxNSA2NTUzNSBmDQowMDAwMDAwMDE2IDY1NTM1IGYNCjAwMDAwMDAwMTcgNjU1MzUgZg0KMDAwMDAwMDAwMCA2NTUzNSBmDQowMDAwMDAxOTM5IDAwMDAwIG4NCjAwMDAwMDIxNjAgMDAwMDAgbg0KMDAwMDAyOTI3NCAwMDAwMCBuDQowMDAwMDMyNDQ1IDAwMDAwIG4NCjAwMDAwMzI0OTAgMDAwMDAgbg0KdHJhaWxlcg0KPDwvU2l6ZSAyMy9Sb290IDEgMCBSL0luZm8gOSAwIFIvSURbPENFN0EzMUQ2REM5MkEyNDlCNzY5RjAzQUMwNDk5NjBCPjxDRTdBMzFENkRDOTJBMjQ5Qjc2OUYwM0FDMDQ5OTYwQj5dID4+DQpzdGFydHhyZWYNCjMyNzc0DQolJUVPRg0KeHJlZg0KMCAwDQp0cmFpbGVyDQo8PC9TaXplIDIzL1Jvb3QgMSAwIFIvSW5mbyA5IDAgUi9JRFs8Q0U3QTMxRDZEQzkyQTI0OUI3NjlGMDNBQzA0OTk2MEI+PENFN0EzMUQ2REM5MkEyNDlCNzY5RjAzQUMwNDk5NjBCPl0gL1ByZXYgMzI3NzQvWFJlZlN0bSAzMjQ5MD4+DQpzdGFydHhyZWYNCjMzMzkwDQolJUVPRg==";

            // Arrange
            LaboratoryReport response = new()
            {
                Report = expectedPdf,
            };

            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetLaboratoryReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<LaboratoryReport> actualResult = await labDelegate.GetLabReportAsync(ReportId, Hdid, AccessToken, true);

            // Verify
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(expectedPdf, actualResult.ResourcePayload!.Report);
        }

        /// <summary>
        /// Get Covid19 Laboratory Report handles HttpStatusCode.NoContent.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetCovid19LabReportHandlesHttpStatusCodeNoContent()
        {
            string expectedMessage = "Laboratory Report not found";

            // Arrange
            ApiException mockException = MockRefitExceptionHelper.CreateApiException(HttpStatusCode.NoContent, HttpMethod.Get);
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetLaboratoryReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(mockException);
            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<LaboratoryReport> actualResult = await labDelegate.GetLabReportAsync(ReportId, Hdid, AccessToken, true);

            // Verify
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError!.ResultMessage);
        }

        /// <summary>
        /// Get Covid19 Laboratory Report handles ProblemDetailsException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetCovid19LabReportHandlesProblemDetailsException()
        {
            string expectedMessage = $"Status: {HttpStatusCode.Unauthorized}. Error retrieving Laboratory Report";

            // Arrange
            ApiException mockException = MockRefitExceptionHelper.CreateApiException(HttpStatusCode.Unauthorized, HttpMethod.Get);
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetLaboratoryReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(mockException);

            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<LaboratoryReport> actualResult = await labDelegate.GetLabReportAsync(ReportId, Hdid, AccessToken, true);

            // Verify
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError!.ResultMessage);
        }

        /// <summary>
        /// Get Covid19 Laboratory Report handles HttpRequestException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetCovid19LabReportHandlesHttpRequestException()
        {
            string expectedMessage = $"Status: {HttpStatusCode.InternalServerError}. Error retrieving Laboratory Report";

            // Arrange
            HttpRequestException mockException = MockRefitExceptionHelper.CreateHttpRequestException("Internal Server Error", HttpStatusCode.InternalServerError);
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetLaboratoryReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(mockException);

            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<LaboratoryReport> actualResult = await labDelegate.GetLabReportAsync(ReportId, Hdid, AccessToken, true);

            // Verify
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError!.ResultMessage);
        }

        /// <summary>
        /// Get Plis Laboratory Report.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetPlisLabReport()
        {
            string expectedPdf =
                "JVBERi0xLjcNCiW1tbW1DQoxIDAgb2JqDQo8PC9UeXBlL0NhdGFsb2cvUGFnZXMgMiAwIFIvTGFuZyhlbi1VUykgL1N0cnVjdFRyZWVSb290IDEwIDAgUi9NYXJrSW5mbzw8L01hcmtlZCB0cnVlPj4vTWV0YWRhdGEgMjAgMCBSL1ZpZXdlclByZWZlcmVuY2VzIDIxIDAgUj4+DQplbmRvYmoNCjIgMCBvYmoNCjw8L1R5cGUvUGFnZXMvQ291bnQgMS9LaWRzWyAzIDAgUl0gPj4NCmVuZG9iag0KMyAwIG9iag0KPDwvVHlwZS9QYWdlL1BhcmVudCAyIDAgUi9SZXNvdXJjZXM8PC9Gb250PDwvRjEgNSAwIFI+Pi9FeHRHU3RhdGU8PC9HUzcgNyAwIFIvR1M4IDggMCBSPj4vUHJvY1NldFsvUERGL1RleHQvSW1hZ2VCL0ltYWdlQy9JbWFnZUldID4+L01lZGlhQm94WyAwIDAgNjEyIDc5Ml0gL0NvbnRlbnRzIDQgMCBSL0dyb3VwPDwvVHlwZS9Hcm91cC9TL1RyYW5zcGFyZW5jeS9DUy9EZXZpY2VSR0I+Pi9UYWJzL1MvU3RydWN0UGFyZW50cyAwPj4NCmVuZG9iag0KNCAwIG9iag0KPDwvRmlsdGVyL0ZsYXRlRGVjb2RlL0xlbmd0aCAxOTM+Pg0Kc3RyZWFtDQp4nK2OywrCMBBF94H8w10mgmmStqaB4qKtiqLgI+BCXFSoj4Vv/x9j6UIXrnQYBi4zzDkIpkjTYJIPC8huF1mR40qJFPJVVmlIdPw0VuNWUbJs4URJ5igJ+gpKCRnBbSlR/k5CwWghdQQjrYj95ujvBguD3d3/xK5OSZMGlKyY2x/u8F2CWzbm7ZCVGx4xzHnCKr6GG1HS87QZJb9oqdAKrd+1aptG4sJDdubtmN0ef0TGiQi/EfHJQW+S4wmjfEzQDQplbmRzdHJlYW0NCmVuZG9iag0KNSAwIG9iag0KPDwvVHlwZS9Gb250L1N1YnR5cGUvVHJ1ZVR5cGUvTmFtZS9GMS9CYXNlRm9udC9CQ0RFRUUrQ2FsaWJyaS9FbmNvZGluZy9XaW5BbnNpRW5jb2RpbmcvRm9udERlc2NyaXB0b3IgNiAwIFIvRmlyc3RDaGFyIDMyL0xhc3RDaGFyIDExNi9XaWR0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czpkYz0iaHR0cDovL3B1cmwub3JnL2RjL2VsZW1lbnRzLzEuMS8iPgo8ZGM6Y3JlYXRvcj48cmRmOlNlcT48cmRmOmxpPlN0ZXBoZW4gTGF3czwvcmRmOmxpPjwvcmRmOlNlcT48L2RjOmNyZWF0b3I+PC9yZGY6RGVzY3JpcHRpb24+CjxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiICB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iPgo8eG1wOkNyZWF0b3JUb29sPk1pY3Jvc29mdMKuIFdvcmQgZm9yIE1pY3Jvc29mdCAzNjU8L3htcDpDcmVhdG9yVG9vbD48eG1wOkNyZWF0ZURhdGU+MjAyMC0wNS0xMVQxODoxMDo0Ni0wNzowMDwveG1wOkNyZWF0ZURhdGU+PHhtcDpNb2RpZnlEYXRlPjIwMjAtMDUtMTFUMTg6MTA6NDYtMDc6MDA8L3htcDpNb2RpZnlEYXRlPjwvcmRmOkRlc2NyaXB0aW9uPgo8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiAgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iPgo8eG1wTU06RG9jdW1lbnRJRD51dWlkOkQ2MzE3QUNFLTkyREMtNDlBMi1CNzY5LUYwM0FDMDQ5OTYwQjwveG1wTU06RG9jdW1lbnRJRD48eG1wTU06SW5zdGFuY2VJRD51dWlkOkQ2MzE3QUNFLTkyREMtNDlBMi1CNzY5LUYwM0FDMDQ5OTYwQjwveG1wTU06SW5zdGFuY2VJRD48L3JkZjpEZXNjcmlwdGlvbj4KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCjwvcmRmOlJERj48L3g6eG1wbWV0YT48P3hwYWNrZXQgZW5kPSJ3Ij8+DQplbmRzdHJlYW0NCmVuZG9iag0KMjEgMCBvYmoNCjw8L0Rpc3BsYXlEb2NUaXRsZSB0cnVlPj4NCmVuZG9iag0KMjIgMCBvYmoNCjw8L1R5cGUvWFJlZi9TaXplIDIyL1dbIDEgNCAyXSAvUm9vdCAxIDAgUi9JbmZvIDkgMCBSL0lEWzxDRTdBMzFENkRDOTJBMjQ5Qjc2OUYwM0FDMDQ5OTYwQj48Q0U3QTMxRDZEQzkyQTI0OUI3NjlGMDNBQzA0OTk2MEI+XSAvRmlsdGVyL0ZsYXRlRGVjb2RlL0xlbmd0aCA4NT4+DQpzdHJlYW0NCnicY2AAgv//GYGkIAMDiFoGoe6BKcZnYIrpI5hingmmWDog1F4I9QkoD9bOBKGYIRQLhGKFUIwQCqqSDaiPjR2snX0ymOIoAFNFUWCqDmg0ADXHDBsNCmVuZHN0cmVhbQ0KZW5kb2JqDQp4cmVmDQowIDIzDQowMDAwMDAwMDEwIDY1NTM1IGYNCjAwMDAwMDAwMTcgMDAwMDAgbg0KMDAwMDAwMDE2NiAwMDAwMCBuDQowMDAwMDAwMjIyIDAwMDAwIG4NCjAwMDAwMDA0ODYgMDAwMDAgbg0KMDAwMDAwMDc1MyAwMDAwMCBuDQowMDAwMDAwOTIxIDAwMDAwIG4NCjAwMDAwMDExNjAgMDAwMDAgbg0KMDAwMDAwMTIxMyAwMDAwMCBuDQowMDAwMDAxMjY2IDAwMDAwIG4NCjAwMDAwMDAwMTEgNjU1MzUgZg0KMDAwMDAwMDAxMiA2NTUzNSBmDQowMDAwMDAwMDEzIDY1NTM1IGYNCjAwMDAwMDAwMTQgNjU1MzUgZg0KMDAwMDAwMDAxNSA2NTUzNSBmDQowMDAwMDAwMDE2IDY1NTM1IGYNCjAwMDAwMDAwMTcgNjU1MzUgZg0KMDAwMDAwMDAwMCA2NTUzNSBmDQowMDAwMDAxOTM5IDAwMDAwIG4NCjAwMDAwMDIxNjAgMDAwMDAgbg0KMDAwMDAyOTI3NCAwMDAwMCBuDQowMDAwMDMyNDQ1IDAwMDAwIG4NCjAwMDAwMzI0OTAgMDAwMDAgbg0KdHJhaWxlcg0KPDwvU2l6ZSAyMy9Sb290IDEgMCBSL0luZm8gOSAwIFIvSURbPENFN0EzMUQ2REM5MkEyNDlCNzY5RjAzQUMwNDk5NjBCPjxDRTdBMzFENkRDOTJBMjQ5Qjc2OUYwM0FDMDQ5OTYwQj5dID4+DQpzdGFydHhyZWYNCjMyNzc0DQolJUVPRg0KeHJlZg0KMCAwDQp0cmFpbGVyDQo8PC9TaXplIDIzL1Jvb3QgMSAwIFIvSW5mbyA5IDAgUi9JRFs8Q0U3QTMxRDZEQzkyQTI0OUI3NjlGMDNBQzA0OTk2MEI+PENFN0EzMUQ2REM5MkEyNDlCNzY5RjAzQUMwNDk5NjBCPl0gL1ByZXYgMzI3NzQvWFJlZlN0bSAzMjQ5MD4+DQpzdGFydHhyZWYNCjMzMzkwDQolJUVPRg==";

            // Arrange
            LaboratoryReport response = new()
            {
                Report = expectedPdf,
            };

            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetPlisLaboratoryReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<LaboratoryReport> actualResult = await labDelegate.GetLabReportAsync(ReportId, Hdid, AccessToken, false);

            // Verify
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(expectedPdf, actualResult.ResourcePayload!.Report);
        }

        /// <summary>
        /// Get Plis Laboratory Report handles HttpStatusCode.NoContent.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPlisLabReportHandlesHttpStatusCodeNoContent()
        {
            string expectedMessage = "Laboratory Report not found";

            // Arrange
            ApiException mockException = MockRefitExceptionHelper.CreateApiException(HttpStatusCode.NoContent, HttpMethod.Get);
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetPlisLaboratoryReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(mockException);

            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<LaboratoryReport> actualResult = await labDelegate.GetLabReportAsync(ReportId, Hdid, AccessToken, false);

            // Verify
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError!.ResultMessage);
        }

        /// <summary>
        /// Get Plis Laboratory Report handles ProblemDetailsException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPlisLabReportHandlesProblemDetailsException()
        {
            string expectedMessage = $"Status: {HttpStatusCode.Unauthorized}. Error retrieving Laboratory Report";

            // Arrange
            ApiException mockException = MockRefitExceptionHelper.CreateApiException(HttpStatusCode.Unauthorized, HttpMethod.Get);
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetPlisLaboratoryReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(mockException);

            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<LaboratoryReport> actualResult = await labDelegate.GetLabReportAsync(ReportId, Hdid, AccessToken, false);

            // Verify
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError!.ResultMessage);
        }

        /// <summary>
        /// Get Plis Laboratory Report handles HttpRequestException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetPlisLabReportHandlesHttpRequestException()
        {
            string expectedMessage = $"Status: {HttpStatusCode.InternalServerError}. Error retrieving Laboratory Report";

            // Arrange
            HttpRequestException mockException = MockRefitExceptionHelper.CreateHttpRequestException("Internal Server Error", HttpStatusCode.InternalServerError);
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetPlisLaboratoryReportAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(mockException);

            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<LaboratoryReport> actualResult = await labDelegate.GetLabReportAsync(ReportId, Hdid, AccessToken, false);

            // Verify
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError!.ResultMessage);
        }

        /// <summary>
        /// GetLaboratorySummary.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetLaboratorySummary()
        {
            // Arrange
            PhsaResult<PhsaLaboratorySummary> response = new()
            {
                LoadState = new()
                {
                    RefreshInProgress = false,
                    BackOffMilliseconds = 0,
                },
                Result = new(),
            };

            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetPlisLaboratorySummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<PhsaResult<PhsaLaboratorySummary>> actualResult = await labDelegate.GetLaboratorySummaryAsync(Hdid, AccessToken);

            // Verify
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetLaboratorySummary handles HttpStatusCode.NoContent.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetLaboratorySummaryHandlesHttpStatusCodeNoContent()
        {
            // Arrange
            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            ApiException mockException = MockRefitExceptionHelper.CreateApiException(HttpStatusCode.NoContent, HttpMethod.Get);
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetPlisLaboratorySummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(mockException);

            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<PhsaResult<PhsaLaboratorySummary>> actualResult = await labDelegate.GetLaboratorySummaryAsync(Hdid, AccessToken);

            // Verify
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload);
            Assert.NotNull(actualResult.ResourcePayload.Result);
        }

        /// <summary>
        /// GetLaboratorySummary handles ProblemDetailsException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetLaboratorySummaryHandles()
        {
            string expectedMessage = $"Status: {HttpStatusCode.Unauthorized}. Error while retrieving Laboratory Summary";

            // Arrange
            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            ApiException mockException = MockRefitExceptionHelper.CreateApiException(HttpStatusCode.Unauthorized, HttpMethod.Get);
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetPlisLaboratorySummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(mockException);

            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<PhsaResult<PhsaLaboratorySummary>> actualResult = await labDelegate.GetLaboratorySummaryAsync(Hdid, AccessToken);

            // Verify
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError!.ResultMessage);
        }

        /// <summary>
        /// GetLaboratorySummary handles HttpRequestException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetLaboratorySummaryHandleHttpRequestException()
        {
            string expectedMessage = $"Status: {HttpStatusCode.InternalServerError}. Error while retrieving Laboratory Summary";

            // Arrange
            Mock<ILogger<RestLaboratoryDelegate>> mockLogger = new();
            HttpRequestException mockException = MockRefitExceptionHelper.CreateHttpRequestException("Internal Server Error", HttpStatusCode.InternalServerError);
            Mock<ILaboratoryApi> mockLaboratoryApi = new();
            mockLaboratoryApi.Setup(s => s.GetPlisLaboratorySummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(mockException);

            ILaboratoryDelegate labDelegate = new RestLaboratoryDelegate(
                mockLogger.Object,
                mockLaboratoryApi.Object,
                this.configuration);

            // Act
            RequestResult<PhsaResult<PhsaLaboratorySummary>> actualResult = await labDelegate.GetLaboratorySummaryAsync(Hdid, AccessToken);

            // Verify
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(expectedMessage, actualResult.ResultError!.ResultMessage);
        }

        /// <summary>
        /// Tests the mapping from PHSA Covid19 Test to Health Gateway model.
        /// </summary>
        /// <param name="testStatus">The test status.</param>
        /// <param name="result">What ResultReady should be set to based on the test status.</param>
        [Theory]
        [InlineData("Final", true)]
        [InlineData("Corrected", true)]
        [InlineData("Amended", true)]
        [InlineData("", false)]
        public void Covid19TestConverter(string testStatus, bool result)
        {
            PhsaCovid19Test phsaData = new()
            {
                Id = Guid.NewGuid(),
                TestType = "testtype",
                OutOfRange = true,
                CollectedDateTime = DateTime.Now,
                TestStatus = testStatus,
                ResultDescription = { "Description" },
                Loinc = "loinc",
                LoincName = "loincname",
                ReceivedDateTime = DateTime.Now,
                ResultDateTime = DateTime.Now,
            };
            Covid19Test expected = new()
            {
                Id = phsaData.Id,
                TestType = phsaData.TestType,
                OutOfRange = phsaData.OutOfRange,
                CollectedDateTime = phsaData.CollectedDateTime,
                TestStatus = phsaData.TestStatus,
                ResultDescription = phsaData.ResultDescription,
                Loinc = phsaData.Loinc,
                LoincName = phsaData.LoincName,
                ReceivedDateTime = phsaData.ReceivedDateTime,
                ResultDateTime = phsaData.ResultDateTime,
                ResultReady = result,
            };
            Covid19Test actual = this.autoMapper.Map<PhsaCovid19Test, Covid19Test>(phsaData);
            expected.ShouldDeepEqual(actual);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .Build();
        }
    }
}
