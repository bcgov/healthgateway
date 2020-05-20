﻿// -------------------------------------------------------------------------
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
    using Hangfire;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingsJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="notificationSettingsDelegate">The email delegate to use.</param>
        public NotificationSettingsJob(IConfiguration configuration, ILogger<NotificationSettingsJob> logger, INotificationSettingsDelegate notificationSettingsDelegate)
        {
            this.configuration = configuration!;
            this.logger = logger;
            this.notificationSettingsDelegate = notificationSettingsDelegate;
        }

        /// <inheritdoc />
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public async void PushNotificationSettings(NotificationSettingsRequest notificationSettings, string bearerToken)
        {
            this.logger.LogTrace($"Queueing Notification Settings push to PHSA...");
            RequestResult<NotificationSettingsResponse> retVal = await this.notificationSettingsDelegate.SetNotificationSettings(notificationSettings, bearerToken).ConfigureAwait(true);
            if (retVal.ResultStatus != HealthGateway.Common.Constants.ResultType.Success)
            {
                throw new ApplicationException($"Unable to send Notification Settings to PHSA, Error: {retVal.ResultMessage}");
            }

            this.logger.LogDebug($"Finished queueing Notification Settings push.");
        }
    }
}
