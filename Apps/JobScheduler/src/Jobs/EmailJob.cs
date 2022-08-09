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
    using Hangfire;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Database.Delegates;
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using MimeKit;
    using MimeKit.Text;

    /// <inheritdoc/>
    public class EmailJob : IEmailJob
    {
        private const int ConcurrencyTimeout = 5 * 60; // 5 minutes
        private readonly ILogger<EmailJob> logger;
        private readonly IEmailDelegate emailDelegate;
        private readonly string host;
        private readonly int port;
        private readonly int retryFetchSize;
        private readonly int maxRetries;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="emailDelegate">The email delegate to use.</param>
        public EmailJob(IConfiguration configuration, ILogger<EmailJob> logger, IEmailDelegate emailDelegate)
        {
            Contract.Requires(configuration != null && emailDelegate != null);
            this.logger = logger;
            this.emailDelegate = emailDelegate!;
            IConfigurationSection section = configuration!.GetSection("Smtp");
            this.host = section.GetValue<string>("Host");
            this.port = section.GetValue<int>("Port");
            section = configuration.GetSection("EmailJob");
            this.maxRetries = section.GetValue("MaxRetries", 9);
            this.retryFetchSize = section.GetValue("MaxRetryFetchSize", 250);
        }

        /// <inheritdoc/>
        public void SendEmail(Guid emailId)
        {
            this.logger.LogTrace($"Sending email... {emailId}");
            Email? email = this.emailDelegate.GetNewEmail(emailId);
            if (email != null)
            {
                this.SendEmail(email);
            }
            else
            {
                this.logger.LogInformation($"Email {emailId} was not returned from DB, skipping.");
            }

            this.logger.LogDebug("Finished sending email.");
        }

        /// <inheritdoc/>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void SendLowPriorityEmails()
        {
            this.logger.LogDebug($"Sending low priority emails... Looking for up to {this.retryFetchSize} emails to send");
            IList<Email> resendEmails = this.emailDelegate.GetLowPriorityEmail(this.retryFetchSize);
            this.ProcessEmails(resendEmails);
            this.logger.LogDebug("Finished sending low priority emails.");
        }

        /// <inheritdoc/>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void SendStandardPriorityEmails()
        {
            this.logger.LogDebug($"Sending standard priority emails... Looking for up to {this.retryFetchSize} emails to send");
            IList<Email> resendEmails = this.emailDelegate.GetStandardPriorityEmail(this.retryFetchSize);
            this.ProcessEmails(resendEmails);
            this.logger.LogDebug("Finished sending standard priority emails.");
        }

        /// <inheritdoc/>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void SendHighPriorityEmails()
        {
            this.logger.LogDebug($"Sending high priority emails... Looking for up to {this.retryFetchSize} emails to send");
            IList<Email> resendEmails = this.emailDelegate.GetHighPriorityEmail(this.retryFetchSize);
            this.ProcessEmails(resendEmails);
            this.logger.LogDebug("Finished sending high priority emails");
        }

        /// <inheritdoc/>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void SendUrgentPriorityEmails()
        {
            this.logger.LogDebug($"Sending urgent priority emails... Looking for up to {this.retryFetchSize} emails to send");
            IList<Email> resendEmails = this.emailDelegate.GetUrgentPriorityEmail(this.retryFetchSize);
            this.ProcessEmails(resendEmails);
            this.logger.LogDebug("Finished sending urgent priority emails.");
        }

        private static MimeMessage PrepareMessage(Email email)
        {
            MimeMessage msg = new();
            msg.From.Add(new MailboxAddress("Health Gateway", email.From));
            msg.To.Add(MailboxAddress.Parse(email.To));
            msg.Subject = email.Subject;
            msg.Body = new TextPart(email.FormatCode == EmailFormat.HTML ? TextFormat.Html : TextFormat.Plain)
            {
                Text = email.Body,
            };
            return msg;
        }

        private void ProcessEmails(IList<Email> resendEmails)
        {
            if (resendEmails.Count > 0)
            {
                this.logger.LogInformation($"Found {resendEmails.Count} emails to send");
                foreach (Email email in resendEmails)
                {
#pragma warning disable CA1031 //We want to catch exception.
                    try
                    {
                        this.SendEmail(email);
                    }
                    catch (Exception e)
                    {
                        // log the exception as a warning but we can continue
                        this.logger.LogWarning($"Error while sending {email.Id} - skipping for now\n{e.ToString()}");
                    }
#pragma warning restore CA1031 // Restore warnings.
                }
            }
        }

        private void SendEmail(Email email)
        {
            Exception? caught = null;
            email.Attempts++;
            try
            {
                using (SmtpClient smtpClient = new())
                {
                    try
                    {
                        smtpClient.Connect(this.host, this.port, SecureSocketOptions.None);
                        try
                        {
                            using MimeMessage message = PrepareMessage(email);
                            smtpClient.Send(message);
                            email.SmtpStatusCode = (int)SmtpStatusCode.Ok;
                            email.EmailStatusCode = EmailStatus.Processed;
                            email.SentDateTime = DateTime.UtcNow;
                            this.emailDelegate.UpdateEmail(email);
                        }
                        catch (SmtpCommandException e)
                        {
                            caught = e;
                            this.logger.LogError($"Unexpected error while sending email {email.Id}, SMTP Error = {email.SmtpStatusCode}\n{e.ToString()}");
                        }

                        smtpClient.Disconnect(true);
                    }
                    catch (SmtpCommandException e)
                    {
                        caught = e;
                        this.logger.LogError($"Unexpected error while connecting to SMTP Server to send email {email.Id}, SMTP Error = {email.SmtpStatusCode}\n{e.ToString()}");
                    }
                }
            }
#pragma warning disable CA1031 // Disable catching exception as we want to update the DB in all cases.
            catch (Exception e)
            {
                caught = e;
                this.logger.LogError($"Unexpected error while sending email {email.Id}", e);
            }
#pragma warning restore CA1031 // Restore check

            if (caught != null)
            {
                email.LastRetryDateTime = DateTime.UtcNow;
                email.EmailStatusCode = email.Attempts < this.maxRetries ? EmailStatus.Pending : EmailStatus.Error;

                SmtpCommandException? smtpCommandException = caught as SmtpCommandException;
                if (smtpCommandException != null)
                {
                    email.SmtpStatusCode = (int)smtpCommandException.StatusCode;
                }

                this.emailDelegate.UpdateEmail(email);

                // Rethrow our exception, Hangfire will catch it and re-schedule the job.
                throw caught;
            }
        }
    }
}
