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
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Refit;
    using Xunit;

    /// <summary>
    /// Unit tests for RestNotificationSettingsDelegate.
    /// </summary>
    public class NotificationSettingsDelegateTests
    {
        /// <summary>
        /// SetNotificationSettingsAsync - Happy path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnSuccessForSetNotificationSettingsAsync()
        {
            NotificationSettingsRequest request = GetNotificationSettingsRequest();
            NotificationSettingsResponse response = new(request);

            RequestResult<NotificationSettingsResponse> expectedResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = response,
                TotalResultCount = 1,
            };

            Mock<ILogger<RestNotificationSettingsDelegate>> mockLogger = new();
            Mock<INotificationSettingsApi> mockNotificationSettingsApi = new();
            mockNotificationSettingsApi.Setup(s => s.SetNotificationSettingsAsync(It.IsAny<NotificationSettingsRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(response);

            RestNotificationSettingsDelegate notificationSettingsDelegate = new(mockLogger.Object, mockNotificationSettingsApi.Object);

            RequestResult<NotificationSettingsResponse> actualResult =
                await notificationSettingsDelegate.SetNotificationSettingsAsync(request, string.Empty).ConfigureAwait(true);

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// SetNotificationSettingsAsync - Handle HTTP errors.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnErrorForSetNotificationSettingsAsyncHttpError()
        {
            NotificationSettingsRequest request = GetNotificationSettingsRequest();
            RequestResult<NotificationSettingsResponse> expectedResult = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = "Error while sending notification settings to PHSA",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                },
            };

            Mock<ILogger<RestNotificationSettingsDelegate>> mockLogger = new();
            Mock<INotificationSettingsApi> mockNotificationSettingsApi = new();
            mockNotificationSettingsApi.Setup(s => s.SetNotificationSettingsAsync(It.IsAny<NotificationSettingsRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException(null, null, HttpStatusCode.BadRequest));

            RestNotificationSettingsDelegate notificationSettingsDelegate = new(mockLogger.Object, mockNotificationSettingsApi.Object);

            RequestResult<NotificationSettingsResponse> actualResult =
                await notificationSettingsDelegate.SetNotificationSettingsAsync(request, string.Empty).ConfigureAwait(true);

            expectedResult.ShouldDeepEqual(actualResult);
        }

        /// <summary>
        /// SetNotificationSettingsAsync - Handle Refit exceptions.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldReturnErrorForSetNotificationSettingsAsyncException()
        {
            NotificationSettingsRequest request = GetNotificationSettingsRequest();
            RequestResult<NotificationSettingsResponse> expectedResult = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = "Error while sending notification settings to PHSA",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                },
            };

            Mock<ILogger<RestNotificationSettingsDelegate>> mockLogger = new();
            Mock<INotificationSettingsApi> mockNotificationSettingsApi = new();

            using HttpRequestMessage requestMessage = new();
            using HttpResponseMessage responseMessage = new(HttpStatusCode.NotFound);
            ApiException apiException = await ApiException.Create(requestMessage, HttpMethod.Put, responseMessage, new()).ConfigureAwait(true);

            mockNotificationSettingsApi.Setup(s => s.SetNotificationSettingsAsync(It.IsAny<NotificationSettingsRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(apiException);

            RestNotificationSettingsDelegate notificationSettingsDelegate = new(mockLogger.Object, mockNotificationSettingsApi.Object);

            RequestResult<NotificationSettingsResponse> actualResult =
                await notificationSettingsDelegate.SetNotificationSettingsAsync(request, string.Empty).ConfigureAwait(true);

            expectedResult.ShouldDeepEqual(actualResult);
        }

        private static NotificationSettingsRequest GetNotificationSettingsRequest()
        {
            return new NotificationSettingsRequest
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
                SmsVerificationCode = "1234",
            };
        }
    }
}
