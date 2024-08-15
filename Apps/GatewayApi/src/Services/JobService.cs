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
namespace HealthGateway.GatewayApi.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc/>
    /// <param name="configuration">The injected configuration.</param>
    /// <param name="emailQueueService">The injected service to queue emails.</param>
    /// <param name="messageSender">The injected message sender.</param>
    /// <param name="notificationSettingsService">The injected notifications settings service.</param>
    public class JobService(IConfiguration configuration, IEmailQueueService emailQueueService, IMessageSender messageSender, INotificationSettingsService notificationSettingsService) : IJobService
    {
        private readonly EmailTemplateConfig emailTemplateConfig = configuration.GetSection(EmailTemplateConfig.ConfigurationSectionKey).Get<EmailTemplateConfig>() ?? new();

        /// <inheritdoc/>
        public async Task NotifyAccountCreationAsync(string hdid, CancellationToken ct = default)
        {
            IEnumerable<MessageEnvelope> messages = [new MessageEnvelope(new AccountCreatedEvent(hdid, DateTime.UtcNow), hdid)];
            await messageSender.SendAsync(messages, ct);
        }

        /// <inheritdoc/>
        public async Task NotifyEmailVerificationAsync(string hdid, string email, CancellationToken ct = default)
        {
            IEnumerable<MessageEnvelope> messages = [new(new NotificationChannelVerifiedEvent(hdid, NotificationChannel.Email, email), hdid)];
            await messageSender.SendAsync(messages, ct);
        }

        /// <inheritdoc/>
        public async Task NotifySmsVerificationAsync(string hdid, string smsNumber, CancellationToken ct = default)
        {
            IEnumerable<MessageEnvelope> messages = [new(new NotificationChannelVerifiedEvent(hdid, NotificationChannel.Sms, smsNumber), hdid)];
            await messageSender.SendAsync(messages, ct);
        }

        /// <inheritdoc/>
        public async Task SendEmailAsync(Email email, bool shouldCommit = true, CancellationToken ct = default)
        {
            await emailQueueService.QueueNewEmailAsync(email, shouldCommit, ct);
        }

        /// <inheritdoc/>
        public async Task SendEmailAsync(string emailAddress, string emailTemplateName, bool shouldCommit = true, CancellationToken ct = default)
        {
            Dictionary<string, string> keyValues = new() { [EmailTemplateVariable.Host] = this.emailTemplateConfig.Host };
            await emailQueueService.QueueNewEmailAsync(emailAddress, emailTemplateName, keyValues, shouldCommit, ct);
        }

        /// <inheritdoc/>
        public async Task PushNotificationSettingsToPhsaAsync(UserProfile userProfile, string? email, string? smsNumber, string? smsVerificationCode = null, CancellationToken ct = default)
        {
            NotificationSettingsRequest notificationSettingsRequest = new(userProfile, email, smsNumber) { SmsVerificationCode = smsVerificationCode };
            await notificationSettingsService.QueueNotificationSettingsAsync(notificationSettingsRequest, ct);
        }
    }
}
