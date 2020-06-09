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
namespace Healthgateway.JobScheduler.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.AccessManagement.Administration;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class NotificationSettingsJob : INotificationSettingsJob
    {
        private const int ConcurrencyTimeout = 5 * 60; // 5 minutes
        private readonly IConfiguration configuration;
        private readonly ILogger<NotificationSettingsJob> logger;
        private readonly INotificationSettingsDelegate notificationSettingsDelegate;
        private readonly IAuthenticationDelegate authDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingsJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="notificationSettingsDelegate">The email delegate to use.</param>
        /// <param name="authDelegate">The OAuth2 authentication service.</param>
        public NotificationSettingsJob(IConfiguration configuration, ILogger<NotificationSettingsJob> logger, INotificationSettingsDelegate notificationSettingsDelegate, IAuthenticationDelegate authDelegate)
        {
            this.configuration = configuration!;
            this.logger = logger;
            this.notificationSettingsDelegate = notificationSettingsDelegate;
            this.authDelegate = authDelegate;
        }

        /// <inheritdoc />
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void PushNotificationSettings(string notificationSettingsJSON)
        {
            this.logger.LogDebug($"Queueing Notification Settings push to PHSA...");
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                WriteIndented = true,
            };

            NotificationSettingsRequest notificationSettings = JsonSerializer.Deserialize<NotificationSettingsRequest>(notificationSettingsJSON, options);
            string? accessToken = this.authDelegate.AuthenticateAsSystem().AccessToken;

            if (string.IsNullOrEmpty(accessToken))
            {
                this.logger.LogError($"Authenticated as System access token is null or emtpy, Error:\n{accessToken}");
                throw new Exception($"Authenticated as System access token is null or emtpy, Error:\n{accessToken}");
            }
            else
            {
                RequestResult<NotificationSettingsResponse> retVal = Task.Run(async () => await
                                this.notificationSettingsDelegate.SetNotificationSettings(notificationSettings, accessToken).ConfigureAwait(true)).Result;
                if (retVal.ResultStatus != HealthGateway.Common.Constants.ResultType.Success)
                {
                    this.logger.LogError($"Unable to send Notification Settings to PHSA, Error:\n{retVal.ResultMessage}");
                    throw new Exception($"Unable to send Notification Settings to PHSA, Error:\n{retVal.ResultMessage}");
                }
            }

            this.logger.LogDebug($"Finished queueing Notification Settings push.");
        }
    }
}
