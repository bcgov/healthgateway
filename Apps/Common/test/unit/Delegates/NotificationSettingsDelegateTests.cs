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
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// NotificationSettingsDelegate's Unit Tests.
    /// </summary>
    public class NotificationSettingsDelegateTests
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingsDelegateTests"/> class.
        /// </summary>
        public NotificationSettingsDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// GetNotificationSettings - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateGetNotificationSettings200()
        {
            RequestResult<NotificationSettingsResponse> expectedRequestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
            };

            (RequestResult<NotificationSettingsResponse> actualResult, RequestResult<NotificationSettingsResponse> expectedResult)
                = this.GetNotificationSettings(HttpStatusCode.OK, expectedRequestResult);

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// GetNotificationSettings - Unknown Exception.
        /// </summary>
        [Fact]
        public void ValidateGetNotificationSettingsCatchException()
        {
            RequestResult<NotificationSettingsResponse> expectedRequestResult = new()
            {
                ResultStatus = ResultType.Error,
            };

            (RequestResult<NotificationSettingsResponse> actualResult, RequestResult<NotificationSettingsResponse> expectedResult)
                = this.GetNotificationSettings(HttpStatusCode.OK, expectedRequestResult, true);

            Assert.Equal(expectedResult.ResultStatus, actualResult.ResultStatus);
            Assert.Contains("Exception getting Notification Settings:", actualResult.ResultError?.ResultMessage, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// GetNotificationSettings - Bad Request.
        /// </summary>
        [Fact]
        public void ValidateGetNotificationSettingsUnableToConnect()
        {
            RequestResult<NotificationSettingsResponse> expectedRequestResult = new()
            {
                ResultStatus = ResultType.Error,
            };

            (RequestResult<NotificationSettingsResponse> actualResult, _) = this.GetNotificationSettings(HttpStatusCode.BadRequest, expectedRequestResult);

            Assert.Equal($"Unable to connect to Notification Settings Endpoint, HTTP Error {HttpStatusCode.BadRequest}", actualResult.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetNotificationSettings - No Content.
        /// </summary>
        [Fact]
        public void ValidateGetNotificationSettings204()
        {
            RequestResult<NotificationSettingsResponse> expected = new()
            {
                ResourcePayload = new NotificationSettingsResponse(),
                ResultStatus = ResultType.Success,
                TotalResultCount = 0,
            };
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.NoContent,
                Content = new StringContent(string.Empty),
            };
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(
                loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(),
                mockHttpClientService.Object,
                this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.GetNotificationSettings(string.Empty).ConfigureAwait(true)).Result;

            actualResult.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// GetNotificationSettings - Forbidden.
        /// </summary>
        [Fact]
        public void ValidateGetNotificationSettings403()
        {
            RequestResult<NotificationSettingsResponse> expected = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = "DID Claim is missing or can not resolve PHN, HTTP Error Forbidden", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                },
            };
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new StringContent(string.Empty),
            };
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(
                loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(),
                mockHttpClientService.Object,
                this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.GetNotificationSettings(string.Empty).ConfigureAwait(true)).Result;

            expected.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// SetNotificationSettings - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateSetNotificationSettings200()
        {
            RequestResult<NotificationSettingsResponse> expectedRequestResult = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
            };

            (RequestResult<NotificationSettingsResponse> actualResult, RequestResult<NotificationSettingsResponse> expectedResult)
                = this.SetNotificationSettings(HttpStatusCode.OK, expectedRequestResult);

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// SetNotificationSettings - Unknown Exception.
        /// </summary>
        [Fact]
        public void ValidateSetNotificationSettingsCatchException()
        {
            RequestResult<NotificationSettingsResponse> expectedRequestResult = new()
            {
                ResultStatus = ResultType.Error,
            };

            (RequestResult<NotificationSettingsResponse> actualResult, RequestResult<NotificationSettingsResponse> expectedResult)
                = this.SetNotificationSettings(HttpStatusCode.OK, expectedRequestResult, true);

            Assert.Equal(expectedResult.ResultStatus, actualResult.ResultStatus);
            Assert.Contains("Exception getting Notification Settings:", actualResult.ResultError?.ResultMessage, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// SetNotificationSettings - Bad Request.
        /// </summary>
        [Fact]
        public void ValidateSetNotificationSettingsUnableToConnect()
        {
            RequestResult<NotificationSettingsResponse> expectedRequestResult = new()
            {
                ResultStatus = ResultType.Error,
            };

            (RequestResult<NotificationSettingsResponse> actualResult, _) = this.SetNotificationSettings(HttpStatusCode.BadRequest, expectedRequestResult);

            Assert.Contains("Bad Request, HTTP Error BadRequest", actualResult.ResultError?.ResultMessage, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// SetNotificationSettings - Happy Path (201).
        /// </summary>
        [Fact]
        public void ValidateSetNotificationSettings201()
        {
            RequestResult<NotificationSettingsResponse> expected = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new NotificationSettingsResponse
                {
                    SmsEnabled = true,
                    SmsNumber = "5551231234",
                    SmsScope = new List<NotificationTarget>
                    {
                        NotificationTarget.Covid19,
                    },
                    EmailEnabled = true,
                    EmailAddress = "DrGateway@HealthGateway.gov.bc.ca",
                    EmailScope = new List<NotificationTarget>
                    {
                        NotificationTarget.Covid19,
                    },
                },
                TotalResultCount = 1,
            };
            NotificationSettingsRequest request = new(expected.ResourcePayload)
            {
                SmsVerificationCode = "1234",
            };
            string json = JsonSerializer.Serialize(request);

            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(json),
            };
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(
                loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(),
                mockHttpClientService.Object,
                this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(request, string.Empty).ConfigureAwait(true)).Result;

            expected.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// SetNotificationSettings - Bad Request.
        /// </summary>
        [Fact]
        public void ValidateSetNotificationSettings400()
        {
            string errMsg = "Mocked Error";
            RequestResult<NotificationSettingsResponse> expected = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError
                    { ResultMessage = $"Bad Request, HTTP Error BadRequest\nDetails:\n{errMsg}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa) },
            };
            NotificationSettingsRequest notificationSettings = new()
            {
                SmsEnabled = true,
                SmsNumber = "5551231234",
                SmsScope = new List<NotificationTarget>
                {
                    NotificationTarget.Covid19,
                },
                EmailEnabled = true,
                EmailAddress = "DrGateway@HealthGateway.gov.bc.ca",
                EmailScope = new List<NotificationTarget>
                {
                    NotificationTarget.Covid19,
                },
            };
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(errMsg),
            };
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(
                loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(),
                mockHttpClientService.Object,
                this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(notificationSettings, string.Empty).ConfigureAwait(true)).Result;

            expected.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// SetNotificationSettings - Forbidden.
        /// </summary>
        [Fact]
        public void ValidateSetNotificationSettings403()
        {
            RequestResult<NotificationSettingsRequest> expected = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = "DID Claim is missing or can not resolve PHN, HTTP Error Forbidden", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                },
            };
            NotificationSettingsRequest notificationSettings = new()
            {
                SmsEnabled = true,
                SmsNumber = "5551231234",
                SmsScope = new List<NotificationTarget>
                {
                    NotificationTarget.Covid19,
                },
                EmailEnabled = true,
                EmailAddress = "DrGateway@HealthGateway.gov.bc.ca",
                EmailScope = new List<NotificationTarget>
                {
                    NotificationTarget.Covid19,
                },
            };
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new StringContent(string.Empty),
            };
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(
                loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(),
                mockHttpClientService.Object,
                this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(notificationSettings, string.Empty).ConfigureAwait(true)).Result;

            expected.ShouldDeepEqual(actualResult);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "NotificationSettings:Endpoint", "https://phsahealthgatewayapi.azurewebsites.net/api/v1/Settings/Notification" },
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static Mock<IHttpClientService> GetHttpClientServiceMock(HttpResponseMessage httpResponseMessage)
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

            return mockHttpClientService;
        }

        /// <summary>
        /// Get Notification Settings.
        /// </summary>
        /// <param name="expectedResponseStatusCode">expectedResponseStatusCode.</param>
        /// <param name="expectedRequestResult">expectedRequestResult.</param>
        /// <param name="throwException">Throw exception indicator.</param>
        /// <returns>The notification settings.</returns>
        private (RequestResult<NotificationSettingsResponse> ActualResult, RequestResult<NotificationSettingsResponse> ExpectedRequestResult) GetNotificationSettings(
            HttpStatusCode expectedResponseStatusCode,
            RequestResult<NotificationSettingsResponse> expectedRequestResult,
            bool throwException = false)
        {
            string json =
                @"{""smsEnabled"": true, ""smsCellNumber"": ""5551231234"", ""smsVerified"": true, ""smsScope"": [""COVID19""], ""emailEnabled"": true, ""emailAddress"": ""email@email.blah"", ""emailScope"": [""COVID19""]}";

            expectedRequestResult.ResourcePayload = JsonSerializer.Deserialize<NotificationSettingsResponse>(json);
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = expectedResponseStatusCode,
                Content = throwException ? null : new StringContent(json),
            };
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(
                loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(),
                mockHttpClientService.Object,
                this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.GetNotificationSettings(string.Empty).ConfigureAwait(true)).Result;
            return (actualResult, expectedRequestResult);
        }

        /// <summary>
        /// Get Notification Settings.
        /// </summary>
        /// <param name="expectedResponseStatusCode">expectedResponseStatusCode.</param>
        /// <param name="expectedRequestResult">expectedRequestResult.</param>
        /// <param name="throwException">Throw exception indicator.</param>
        /// <returns>Mocked notification settings.</returns>
        private (RequestResult<NotificationSettingsResponse> ActualResult, RequestResult<NotificationSettingsResponse> ExpectedRequestResult) SetNotificationSettings(
            HttpStatusCode expectedResponseStatusCode,
            RequestResult<NotificationSettingsResponse> expectedRequestResult,
            bool throwException = false)
        {
            expectedRequestResult.ResourcePayload = new NotificationSettingsResponse
            {
                SmsEnabled = true,
                SmsNumber = "5551231234",
                SmsScope = new List<NotificationTarget>
                {
                    NotificationTarget.Covid19,
                },
                EmailEnabled = true,
                EmailAddress = "DrGateway@HealthGateway.gov.bc.ca",
                EmailScope = new List<NotificationTarget>
                {
                    NotificationTarget.Covid19,
                },
            };

            NotificationSettingsRequest request = new(expectedRequestResult.ResourcePayload)
            {
                SmsVerificationCode = "1234",
            };
            string json = JsonSerializer.Serialize(request);
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = expectedResponseStatusCode,
                Content = throwException ? null : new StringContent(json),
            };
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(
                loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(),
                mockHttpClientService.Object,
                this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(request, string.Empty).ConfigureAwait(true)).Result;
            return (actualResult, expectedRequestResult);
        }
    }
}
