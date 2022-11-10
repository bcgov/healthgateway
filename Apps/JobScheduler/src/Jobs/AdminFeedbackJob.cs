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
    using Hangfire;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using MimeKit;
    using MimeKit.Text;

    /// <inheritdoc/>
    public class AdminFeedbackJob : IAdminFeedbackJob
    {
        private const string FeedbackTemplateName = "AdminFeedback";
        private const int ConcurrencyTimeout = 5 * 60; // 5 minutes
        private readonly string adminEmail;
        private readonly IEmailQueueService emailService;
        private readonly IFeedbackDelegate feedBackDelegate;
        private readonly string host;
        private readonly ILogger<AdminFeedbackJob> logger;
        private readonly int port;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminFeedbackJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="feedBackDelegate">The db feedback delegate to use.</param>
        /// <param name="emailService">The email service for template retrieval and parsing.</param>
        public AdminFeedbackJob(
            IConfiguration configuration,
            ILogger<AdminFeedbackJob> logger,
            IFeedbackDelegate feedBackDelegate,
            IEmailQueueService emailService)
        {
            this.logger = logger;
            this.feedBackDelegate = feedBackDelegate;
            this.emailService = emailService;
            IConfigurationSection section = configuration!.GetSection("Smtp");
            this.host = section.GetValue<string>("Host") ?? throw new ArgumentNullException(nameof(configuration), "SMTP Host is null");
            this.port = section.GetValue<int>("Port");
            this.adminEmail = configuration.GetValue<string>("AdminFeedback:AdminEmail") ?? throw new ArgumentNullException(nameof(configuration), "Admin Email is null");
        }

        /// <inheritdoc/>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void SendEmail(ClientFeedback clientFeedback)
        {
            DbResult<UserFeedback> dbResult = this.feedBackDelegate.GetUserFeedback(clientFeedback.UserFeedbackId);
            if (dbResult.Status == DbStatusCode.Read)
            {
                UserFeedback feedback = dbResult.Payload;
                this.logger.LogDebug("Sending Email...");
                using SmtpClient smtpClient = new();
                smtpClient.Connect(this.host, this.port, SecureSocketOptions.None);
                using MimeMessage message = this.PrepareMessage(clientFeedback.Email, feedback);
                smtpClient.Send(message);
                smtpClient.Disconnect(true);
                this.logger.LogDebug("Finished Sending Email...");
            }
            else
            {
                this.logger.LogCritical("Unable to read UserFeedback with id {UserFeedbackId}", clientFeedback.UserFeedbackId);
                throw new InvalidOperationException($"Unable to read UserFeedback with id {clientFeedback.UserFeedbackId}");
            }
        }

        private MimeMessage PrepareMessage(string userEmail, UserFeedback feedback)
        {
            EmailTemplate template = this.emailService.GetEmailTemplate(FeedbackTemplateName);
            if (template == null)
            {
                this.logger.LogCritical($"Email template {FeedbackTemplateName} is null");
                throw new InvalidOperationException($"Email template {FeedbackTemplateName} is null");
            }

            Dictionary<string, string> keyValues = new()
            {
                { "hdid", feedback.UserProfileId },
                { "feedback", feedback.Comment },
            };

            Email email = this.emailService.ProcessTemplate(this.adminEmail, template, keyValues);
            MimeMessage msg = new();
            msg.From.Add(new MailboxAddress("HG Feedback", email.From));
            msg.ReplyTo.Add(MailboxAddress.Parse(userEmail));
            msg.To.Add(MailboxAddress.Parse(email.To));
            msg.Subject = email.Subject;
            msg.Body = new TextPart(TextFormat.Html)
            {
                Text = email.Body,
            };

            return msg;
        }
    }
}
