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
namespace HealthGateway.JobScheduler.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Database.Delegates;
    using HealthGateway.JobScheduler.Api;
    using HealthGateway.JobScheduler.Models;
    using HealthGateway.JobScheduler.Models.Notify;
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using MimeKit;
    using MimeKit.Text;

    /// <summary>
    /// Performs email related batch jobs.
    /// </summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Team decision")]
    public class EmailJob : IEmailJob, IOtherEmailJob
    {
        private const int ConcurrencyTimeout = 5 * 60; // 5 minutes
        private readonly IEmailDelegate emailDelegate;
        private readonly INotifyApi notifyApi;
        private readonly string host;
        private readonly ILogger<EmailJob> logger;
        private readonly int maxRetries;
        private readonly int port;
        private readonly int retryFetchSize;
        private readonly NotifyConfiguration notifyConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="notifyOptions">The Notify options to use.</param>
        /// <param name="emailDelegate">The email delegate to use.</param>
        /// <param name="notifyApi">The GC Notify API for sending email.</param>
        public EmailJob(IConfiguration configuration, ILogger<EmailJob> logger, IOptions<NotifyConfiguration> notifyOptions, IEmailDelegate emailDelegate, INotifyApi notifyApi)
        {
            this.logger = logger;
            this.emailDelegate = emailDelegate;
            this.notifyApi = notifyApi;
            IConfigurationSection section = configuration.GetSection("Smtp");
            this.host = section.GetValue<string>("Host") ?? throw new ArgumentNullException(nameof(configuration), "SMTP Host is null");
            this.port = section.GetValue<int>("Port");
            section = configuration.GetSection("EmailJob");
            this.maxRetries = section.GetValue("MaxRetries", 9);
            this.retryFetchSize = section.GetValue("MaxRetryFetchSize", 250);
            this.notifyConfiguration = notifyOptions.Value;
        }

        /// <inheritdoc/>
        public void SendEmail(Guid emailId)
        {
            this.logger.LogTrace("Sending email... {EmailId}", emailId.ToString());
            Email? email = this.emailDelegate.GetStandardEmail(emailId);
            if (email != null)
            {
                if (this.notifyConfiguration.Enabled)
                {
                    this.SendEmailUsingNotify(email).GetAwaiter().GetResult();
                }
                else
                {
                    this.SendEmail(email);
                }
            }
            else
            {
                this.logger.LogInformation("Email {EmailId} was not returned from DB, skipping", emailId.ToString());
            }

            this.logger.LogDebug("Finished sending email");
        }

        /// <inheritdoc/>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public void SendEmails()
        {
            this.logger.LogDebug("Sending low priority emails... Looking for up to {RetryFetchSize} emails to send", this.retryFetchSize);
            IList<Email> resendEmails = this.emailDelegate.GetUnsentEmails(this.retryFetchSize);
            this.ProcessEmails(resendEmails);
            this.logger.LogDebug("Finished sending low priority emails");
        }

        private static MimeMessage PrepareMessage(Email email)
        {
            MimeMessage msg = new();
            msg.From.Add(new MailboxAddress("Health Gateway", email.From));
            msg.To.Add(MailboxAddress.Parse(email.To));
            msg.Subject = email.Subject;
            msg.Body = new TextPart(email.FormatCode == EmailFormat.Html ? TextFormat.Html : TextFormat.Plain)
            {
                Text = email.Body,
            };
            return msg;
        }

        private void ProcessEmails(IList<Email> resendEmails)
        {
            if (resendEmails.Count > 0)
            {
                this.logger.LogInformation("Found {Count} emails to send", resendEmails.Count);
                foreach (Email email in resendEmails)
                {
                    try
                    {
                        if (this.notifyConfiguration.Enabled)
                        {
                            this.SendEmailUsingNotify(email).GetAwaiter().GetResult();
                        }
                        else
                        {
                            this.SendEmail(email);
                        }
                    }
                    catch (Exception e)
                    {
                        // log the exception as a warning but we can continue
                        this.logger.LogWarning("Error while sending {Id} - skipping for now\n{Exception}", email.Id.ToString(), e.ToString());
                    }
                }
            }
        }

        private async Task SendEmailUsingNotify(Email email)
        {
            email.Attempts++;
            try
            {
                _ = email.Template ?? throw new MissingFieldException("TemplateId is null");
                EmailRequest emailRequest = new()
                {
                    EmailAddress = email.To!,
                    TemplateId = this.notifyConfiguration.Templates[email.Template],
                    Personalization = email.Personalization,
                    Reference = email.Id.ToString("D"),
                };
                EmailResponse response = await this.notifyApi.SendEmail(emailRequest).ConfigureAwait(true);
                email.NotificationId = response.Id;
                email.EmailStatusCode = EmailStatus.Processed;
                email.SentDateTime = DateTime.UtcNow;
                this.emailDelegate.UpdateEmail(email);
            }
            catch (Exception e)
            {
                this.logger.LogError(
                    "Unexpected error while communicating to GC Notify API for email {Id}, Error = {Exception}",
                    email.Id.ToString(),
                    e.ToString());
                email.LastRetryDateTime = DateTime.UtcNow;
                email.EmailStatusCode = email.Attempts < this.maxRetries ? EmailStatus.Pending : EmailStatus.Error;
                this.emailDelegate.UpdateEmail(email);
                throw;
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
                            this.logger.LogError("Unexpected error while sending email {Id}, SMTP Error = {SmtpStatusCode}\n{Exception}", email.Id.ToString(), email.SmtpStatusCode, e.ToString());
                        }

                        smtpClient.Disconnect(true);
                    }
                    catch (SmtpCommandException e)
                    {
                        caught = e;
                        this.logger.LogError(
                            "Unexpected error while connecting to SMTP Server to send email {Id}, SMTP Error = {SmtpStatusCode}\n{Exception}",
                            email.Id.ToString(),
                            email.SmtpStatusCode,
                            e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                caught = e;
                this.logger.LogError("Unexpected error while sending email {Id} {Exception}", email.Id.ToString(), e.ToString());
            }

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
