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
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
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
            var expectedRequestResult = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 1,
            };
            Tuple<RequestResult<NotificationSettingsResponse>, RequestResult<NotificationSettingsResponse>> response = this.GetNotificationSettings(HttpStatusCode.OK, expectedRequestResult);
            var actualResult = response.Item1;
            var expectedResult = response.Item2;
            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// GetNotificationSettings - Unknown Exception.
        /// </summary>
        [Fact]
        public void ValidateGetNotificationSettingsCatchException()
        {
            var expectedRequestResult = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };
            Tuple<RequestResult<NotificationSettingsResponse>, RequestResult<NotificationSettingsResponse>> response = this.GetNotificationSettings(HttpStatusCode.OK, expectedRequestResult, true);
            var actualResult = response.Item1;
            var expectedResult = response.Item2;

            Assert.Equal(expectedResult.ResultStatus, actualResult.ResultStatus);
            Assert.Contains("Exception getting Notification Settings:", actualResult?.ResultError?.ResultMessage, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// GetNotificationSettings - Bad Request.
        /// </summary>
        [Fact]
        public void ValidateGetNotificationSettingsUnableToConnect()
        {
            var expectedRequestResult = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };
            Tuple<RequestResult<NotificationSettingsResponse>, RequestResult<NotificationSettingsResponse>> response = this.GetNotificationSettings(HttpStatusCode.BadRequest, expectedRequestResult);
            var actualResult = response.Item1;
            Assert.Equal($"Unable to connect to Notification Settings Endpoint, HTTP Error {HttpStatusCode.BadRequest}", actualResult?.ResultError?.ResultMessage);
        }

        /// <summary>
        /// GetNotificationSettings - No Content.
        /// </summary>
        [Fact]
        public void ValidateGetNotificationSettings204()
        {
            RequestResult<NotificationSettingsResponse> expected = new RequestResult<NotificationSettingsResponse>()
            {
                ResourcePayload = new NotificationSettingsResponse(),
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 0,
            };
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NoContent,
                Content = new StringContent(string.Empty),
            };
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.GetNotificationSettings(string.Empty).ConfigureAwait(true)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        /// <summary>
        /// GetNotificationSettings - Forbidden.
        /// </summary>
        [Fact]
        public void ValidateGetNotificationSettings403()
        {
            RequestResult<NotificationSettingsResponse> expected = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = "DID Claim is missing or can not resolve PHN, HTTP Error Forbidden", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) },
            };
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new StringContent(string.Empty),
            };
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.GetNotificationSettings(string.Empty).ConfigureAwait(true)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        /// <summary>
        /// SetNotificationSettings - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateSetNotificationSettings200()
        {
            var expectedRequestResult = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                TotalResultCount = 1,
            };
            Tuple<RequestResult<NotificationSettingsResponse>, RequestResult<NotificationSettingsResponse>> response = this.SetNotificationSettings(HttpStatusCode.OK, expectedRequestResult);
            var actualResult = response.Item1;
            var expectedResult = response.Item2;
            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// SetNotificationSettings - Unknown Exception.
        /// </summary>
        [Fact]
        public void ValidateSetNotificationSettingsCatchException()
        {
            var expectedRequestResult = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };
            Tuple<RequestResult<NotificationSettingsResponse>, RequestResult<NotificationSettingsResponse>> response = this.SetNotificationSettings(HttpStatusCode.OK, expectedRequestResult, true);
            var actualResult = response.Item1;
            var expectedResult = response.Item2;

            Assert.Equal(expectedResult.ResultStatus, actualResult.ResultStatus);
            Assert.Contains("Exception getting Notification Settings:", actualResult?.ResultError?.ResultMessage, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// SetNotificationSettings - Bad Request.
        /// </summary>
        [Fact]
        public void ValidateSetNotificationSettingsUnableToConnect()
        {
            var expectedRequestResult = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };
            Tuple<RequestResult<NotificationSettingsResponse>, RequestResult<NotificationSettingsResponse>> response = this.SetNotificationSettings(HttpStatusCode.BadRequest, expectedRequestResult);
            var actualResult = response.Item1;
            Assert.Contains("Bad Request, HTTP Error BadRequest", actualResult?.ResultError?.ResultMessage, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// SetNotificationSettings - Happy Path (201).
        /// </summary>
        [Fact]
        public void ValidateSetNotificationSettings201()
        {
            RequestResult<NotificationSettingsResponse> expected = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new NotificationSettingsResponse()
                {
                    SMSEnabled = true,
                    SMSNumber = "5551231234",
                    SMSScope = new List<NotificationTarget>
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
            NotificationSettingsRequest request = new NotificationSettingsRequest(expected.ResourcePayload);
            request.SMSVerificationCode = "1234";
            string json = JsonSerializer.Serialize(request);

            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(json),
            };
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(request, string.Empty).ConfigureAwait(true)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        /// <summary>
        /// SetNotificationSettings - Bad Request.
        /// </summary>
        [Fact]
        public void ValidateSetNotificationSettings400()
        {
            string errMsg = "Mocked Error";
            RequestResult<NotificationSettingsResponse> expected = new RequestResult<NotificationSettingsResponse>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = $"Bad Request, HTTP Error BadRequest\nDetails:\n{errMsg}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) },
            };
            NotificationSettingsRequest notificationSettings = new NotificationSettingsRequest()
            {
                SMSEnabled = true,
                SMSNumber = "5551231234",
                SMSScope = new List<NotificationTarget>
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
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(errMsg),
            };
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(notificationSettings, string.Empty).ConfigureAwait(true)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        /// <summary>
        /// SetNotificationSettings - Forbidden.
        /// </summary>
        [Fact]
        public void ValidateSetNotificationSettings403()
        {
            RequestResult<NotificationSettingsRequest> expected = new RequestResult<NotificationSettingsRequest>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                ResultError = new RequestResultError() { ResultMessage = "DID Claim is missing or can not resolve PHN, HTTP Error Forbidden", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) },
            };
            NotificationSettingsRequest notificationSettings = new NotificationSettingsRequest()
            {
                SMSEnabled = true,
                SMSNumber = "5551231234",
                SMSScope = new List<NotificationTarget>
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
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new StringContent(string.Empty),
            };
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(notificationSettings, string.Empty).ConfigureAwait(true)).Result;
            Assert.True(actualResult.IsDeepEqual(expected));
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { "NotificationSettings:Endpoint", "https://phsahealthgatewayapi.azurewebsites.net/api/v1/Settings/Notification" },
            };

            return new ConfigurationBuilder()

                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static Mock<IHttpClientService> GetHttpClientServiceMock(HttpResponseMessage httpResponseMessage)
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

            return mockHttpClientService;
        }

        /// <summary>
        /// Get Notification Settings.
        /// </summary>
        /// <param name="expectedResponseStatusCode">expectedResponseStatusCode.</param>
        /// <param name="expectedRequestResult">expectedRequestResult.</param>
        /// <param name="throwException">Throw exception indicator.</param>
        /// <returns>The notification settings.</returns>
        private Tuple<RequestResult<NotificationSettingsResponse>, RequestResult<NotificationSettingsResponse>> GetNotificationSettings(
            HttpStatusCode expectedResponseStatusCode,
            RequestResult<NotificationSettingsResponse> expectedRequestResult,
            bool throwException = false)
        {
            string json = @"{""smsEnabled"": true, ""smsCellNumber"": ""5551231234"", ""smsVerified"": true, ""smsScope"": [""COVID19""], ""emailEnabled"": true, ""emailAddress"": ""email@email.blah"", ""emailScope"": [""COVID19""]}";

            expectedRequestResult.ResourcePayload = JsonSerializer.Deserialize<NotificationSettingsResponse>(json);
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = expectedResponseStatusCode,
                Content = throwException ? null : new StringContent(json),
            };
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.GetNotificationSettings(string.Empty).ConfigureAwait(true)).Result;
            return new Tuple<RequestResult<NotificationSettingsResponse>, RequestResult<NotificationSettingsResponse>>(actualResult, expectedRequestResult);
        }

        /// <summary>
        /// Get Notification Settings.
        /// </summary>
        /// <param name="expectedResponseStatusCode">expectedResponseStatusCode.</param>
        /// <param name="expectedRequestResult">expectedRequestResult.</param>
        /// <param name="throwException">Throw exception indicator.</param>
        /// <returns>Mocked notification settings.</returns>
        private Tuple<RequestResult<NotificationSettingsResponse>, RequestResult<NotificationSettingsResponse>> SetNotificationSettings(
            HttpStatusCode expectedResponseStatusCode,
            RequestResult<NotificationSettingsResponse> expectedRequestResult,
            bool throwException = false)
        {
            expectedRequestResult.ResourcePayload = new NotificationSettingsResponse()
            {
                SMSEnabled = true,
                SMSNumber = "5551231234",
                SMSScope = new List<NotificationTarget>
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

            NotificationSettingsRequest request = new NotificationSettingsRequest(expectedRequestResult.ResourcePayload);
            request.SMSVerificationCode = "1234";
            string json = JsonSerializer.Serialize(request);
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = expectedResponseStatusCode,
                Content = throwException ? null : new StringContent(json),
            };
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            INotificationSettingsDelegate nsDelegate = new RestNotificationSettingsDelegate(loggerFactory.CreateLogger<RestNotificationSettingsDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<NotificationSettingsResponse> actualResult = Task.Run(async () => await nsDelegate.SetNotificationSettings(request, string.Empty).ConfigureAwait(true)).Result;
            return new Tuple<RequestResult<NotificationSettingsResponse>, RequestResult<NotificationSettingsResponse>>(actualResult, expectedRequestResult);
        }
    }
}
