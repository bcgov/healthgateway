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
    using System.Text.Json;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class NotificationSettingsJob : INotificationSettingsJob
    {
        private const int ConcurrencyTimeout = 5 * 60; // 5 minutes
        private const string JobConfigKey = "NotificationSettings";
        private const string JobEnabledKey = "Enabled";
        private const string AuthConfigSectionName = "ClientAuthentication";
        private readonly ILogger<NotificationSettingsJob> logger;
        private readonly INotificationSettingsDelegate notificationSettingsDelegate;
        private readonly IAuthenticationDelegate authDelegate;
        private readonly IEventLogDelegate eventLogDelegate;
        private readonly bool jobEnabled;
        private readonly ClientCredentialsTokenRequest tokenRequest;
        private readonly Uri tokenUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingsJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="notificationSettingsDelegate">The email delegate to use.</param>
        /// <param name="authDelegate">The OAuth2 authentication service.</param>
        /// <param name="eventLogDelegate">The Eventlog delegate.</param>
        public NotificationSettingsJob(
            IConfiguration configuration,
            ILogger<NotificationSettingsJob> logger,
            INotificationSettingsDelegate notificationSettingsDelegate,
            IAuthenticationDelegate authDelegate,
            IEventLogDelegate eventLogDelegate)
        {
            this.logger = logger;
            this.notificationSettingsDelegate = notificationSettingsDelegate;
            this.authDelegate = authDelegate;
            this.eventLogDelegate = eventLogDelegate;
            this.jobEnabled = configuration.GetSection(JobConfigKey).GetValue<bool>(JobEnabledKey, true);

            IConfigurationSection? configSection = configuration?.GetSection(AuthConfigSectionName);
            this.tokenUri = configSection.GetValue<Uri>(@"TokenUri");

            this.tokenRequest = new ClientCredentialsTokenRequest();
            configSection.Bind(this.tokenRequest); // Client ID, Client Secret, Audience, Username, Password
        }

        /// <inheritdoc />
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void PushNotificationSettings(string notificationSettingsJSON)
        {
            this.logger.LogDebug($"Queueing Notification Settings push to PHSA...");
            if (this.jobEnabled)
            {
                NotificationSettingsRequest? notificationSettings = JsonSerializer.Deserialize<NotificationSettingsRequest>(notificationSettingsJSON);
                if (notificationSettings != null)
                {
                    string? accessToken = this.authDelegate.AuthenticateAsUser(this.tokenUri, this.tokenRequest).AccessToken;

                    if (string.IsNullOrEmpty(accessToken))
                    {
                        this.logger.LogError($"Authenticated as User System access token is null or empty, Error:\n{accessToken}");
                        throw new FormatException($"Authenticated as User System access token is null or empty, Error:\n{accessToken}");
                    }
                    else
                    {
                        RequestResult<NotificationSettingsResponse> retVal = Task.Run(async () => await
                                        this.notificationSettingsDelegate.SetNotificationSettings(notificationSettings, accessToken).ConfigureAwait(true)).Result;
                        if (retVal.ResultStatus == ResultType.ActionRequired)
                        {
                            EventLog eventLog = new()
                            {
                                EventSource = this.notificationSettingsDelegate.GetType().Name,
                                EventName = "SMS Rejected",
                                EventDescription = notificationSettings.SMSNumber ?? string.Empty,
                            };
                            this.eventLogDelegate.WriteEventLog(eventLog);
                        }
                        else
                        {
                            if (retVal.ResultStatus != ResultType.Success)
                            {
                                this.logger.LogError($"Unable to send Notification Settings to PHSA, Error:\n{retVal.ResultError?.ResultMessage}");
                                throw new FormatException($"Unable to send Notification Settings to PHSA, Error:\n{retVal.ResultError?.ResultMessage}");
                            }
                        }
                    }
                }
                else
                {
                    this.logger.LogError("Unable to deserialize JSON Notification Settings");
                    throw new FormatException("Unable to deserialize JSON Notification Settings");
                }
            }
            else
            {
                this.logger.LogInformation("Job has been disabled by configuration");
            }

            this.logger.LogDebug($"Finished queueing Notification Settings push.");
        }
    }
}
