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
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using Microsoft.Extensions.Configuration;

    /// <inheritdoc/>
    /// <param name="configuration">Configuration settings.</param>
    /// <param name="messageVerificationDelegate">The injected message verification delegate.</param>
    public partial class MessagingVerificationService(
        IConfiguration configuration,
        IEmailQueueService emailQueueService,
        IMessagingVerificationDelegate messageVerificationDelegate) : IMessagingVerificationService
    {
        private const string EmailConfigExpirySecondsKey = "EmailVerificationExpirySeconds";
        private const int VerificationExpiryDays = 5;
        private const string WebClientConfigSection = "WebClient";

        private readonly int emailVerificationExpirySeconds = configuration.GetSection(WebClientConfigSection).GetValue(EmailConfigExpirySecondsKey, 5);
        private readonly EmailTemplateConfig emailTemplateConfig = configuration.GetSection(EmailTemplateConfig.ConfigurationSectionKey).Get<EmailTemplateConfig>() ?? new();

        /// <inheritdoc/>
        public async Task<MessagingVerification> AddEmailVerificationAsync(string hdid, string email, bool isEmailVerified, bool shouldCommit = true, CancellationToken ct = default)
        {
            MessagingVerification emailVerification = await this.GenerateMessagingVerificationAsync(hdid, email, Guid.NewGuid(), isEmailVerified, ct);
            await messageVerificationDelegate.InsertAsync(emailVerification, shouldCommit, ct);

            return emailVerification;
        }

        /// <inheritdoc/>
        public async Task<MessagingVerification> AddSmsVerificationAsync(string hdid, string smsNumber, bool shouldCommit = true, CancellationToken ct = default)
        {
            MessagingVerification smsVerification = this.GenerateMessagingVerification(hdid, smsNumber);
            await messageVerificationDelegate.InsertAsync(smsVerification, shouldCommit, ct);

            return smsVerification;
        }

        /// <inheritdoc/>
        public MessagingVerification GenerateMessagingVerification(string hdid, string sms, bool sanitize = true)
        {
            if (sanitize)
            {
                sms = SanitizeSms(sms);
            }

            MessagingVerification messagingVerification = new()
            {
                UserProfileId = hdid,
                SmsNumber = sms,
                SmsValidationCode = CreateVerificationCode(),
                VerificationType = MessagingVerificationType.Sms,
                ExpireDate = DateTime.UtcNow.AddDays(VerificationExpiryDays),
            };

            return messagingVerification;
        }

        /// <inheritdoc/>
        public async Task<MessagingVerification> GenerateMessagingVerificationAsync(string hdid, string emailAddress, Guid inviteKey, bool isVerified, CancellationToken ct = default)
        {
            float verificationExpiryHours = (float)this.emailVerificationExpirySeconds / 3600;

            Dictionary<string, string> keyValues = new()
            {
                [EmailTemplateVariable.InviteKey] = inviteKey.ToString(),
                [EmailTemplateVariable.ActivationHost] = this.emailTemplateConfig.Host,
                [EmailTemplateVariable.ExpiryHours] = verificationExpiryHours.ToString("0", CultureInfo.CurrentCulture),
            };

            EmailTemplate emailTemplate = await emailQueueService.GetEmailTemplateAsync(EmailTemplateName.RegistrationTemplate, ct) ??
                                          throw new DatabaseException(ErrorMessages.EmailTemplateNotFound);

            MessagingVerification messagingVerification = new()
            {
                InviteKey = inviteKey,
                UserProfileId = hdid,
                ExpireDate = DateTime.UtcNow.AddSeconds(this.emailVerificationExpirySeconds),
                Email = emailQueueService.ProcessTemplate(emailAddress, emailTemplate, keyValues),
                EmailAddress = emailAddress,
                Validated = isVerified,
            };

            if (isVerified)
            {
                // for verified email addresses, mark verification email as already sent
                messagingVerification.Email.EmailStatusCode = EmailStatus.Processed;
            }

            return messagingVerification;
        }

        /// <summary>
        /// Creates a new 6 digit verification code.
        /// </summary>
        /// <returns>The verification code.</returns>
        private static string CreateVerificationCode()
        {
            using RandomNumberGenerator generator = RandomNumberGenerator.Create();
            byte[] data = new byte[4];
            generator.GetBytes(data);
            return
                BitConverter
                    .ToUInt32(data)
                    .ToString("D6", CultureInfo.InvariantCulture)
                    .Substring(0, 6);
        }

        private static string SanitizeSms(string smsNumber)
        {
            return NonDigitRegex().Replace(smsNumber, string.Empty);
        }

        [GeneratedRegex("[^0-9]")]
        private static partial Regex NonDigitRegex();
    }
}
