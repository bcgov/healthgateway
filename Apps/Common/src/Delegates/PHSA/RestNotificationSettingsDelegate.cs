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
namespace HealthGateway.Common.Delegates.PHSA
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Implementation that uses HTTP to sends notification settings to PHSA.
    /// </summary>
    public class RestNotificationSettingsDelegate : INotificationSettingsDelegate
    {
        private readonly ILogger logger;
        private readonly INotificationSettingsApi notificationSettingsApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestNotificationSettingsDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="notificationSettingsApi">The injected Refit API.</param>
        public RestNotificationSettingsDelegate(
            ILogger<RestNotificationSettingsDelegate> logger,
            INotificationSettingsApi notificationSettingsApi)
        {
            this.logger = logger;
            this.notificationSettingsApi = notificationSettingsApi;
        }

        private static ActivitySource Source { get; } = new(nameof(RestNotificationSettingsDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<NotificationSettingsResponse>> SetNotificationSettingsAsync(NotificationSettingsRequest notificationSettings, string bearerToken)
        {
            using Activity? activity = Source.StartActivity();
            RequestResult<NotificationSettingsResponse> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            this.logger.LogDebug("Sending notification settings to PHSA...");
            this.logger.LogTrace("Bearer token: {BearerToken}", bearerToken);

            try
            {
                NotificationSettingsResponse notificationSettingsResponse = await this.notificationSettingsApi.SetNotificationSettingsAsync(
                        bearerToken,
                        notificationSettings.SubjectHdid,
                        notificationSettings)
                    .ConfigureAwait(true);

                retVal.ResultStatus = ResultType.Success;
                retVal.TotalResultCount = 1;
                retVal.ResourcePayload = notificationSettingsResponse;
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "Error while sending notification settings to PHSA",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
                this.logger.LogError("Unexpected exception in SetNotificationSettings {Exception}", e);
            }

            this.logger.LogDebug("Finished sending notification settings to PHSA");
            return retVal;
        }
    }
}
