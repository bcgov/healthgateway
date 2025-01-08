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
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
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
        public async Task SendEmailAsync(Guid emailId, CancellationToken ct = default)
        {
            this.logger.LogDebug("Sending email {EmailId}", emailId);
            Email? email = await this.emailDelegate.GetStandardEmailAsync(emailId, ct);
            if (email != null)
            {
                if (this.notifyConfiguration.Enabled)
                {
                    await this.SendEmailUsingNotifyAsync(email, ct);
                }
                else
                {
                    await this.SendEmailAsync(email, ct);
                }
            }
            else
            {
                this.logger.LogInformation("Email {EmailId} was not returned from DB, skipping", emailId);
            }

            this.logger.LogDebug("Finished sending email");
        }

        /// <inheritdoc/>
        [DisableConcurrentExecution(ConcurrencyTimeout)]
        public async Task SendEmailsAsync(CancellationToken ct = default)
        {
            this.logger.LogDebug("Sending low priority emails... Looking for up to {RetryFetchSize} emails to send", this.retryFetchSize);
            IList<Email> resendEmails = await this.emailDelegate.GetUnsentEmailsAsync(this.retryFetchSize, ct);
            await this.ProcessEmailsAsync(resendEmails, ct);
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

        private async Task ProcessEmailsAsync(IList<Email> resendEmails, CancellationToken ct)
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
                            await this.SendEmailUsingNotifyAsync(email, ct);
                        }
                        else
                        {
                            await this.SendEmailAsync(email, ct);
                        }
                    }
                    catch (Exception e)
                    {
                        // log the exception as a warning but we can continue
                        this.logger.LogWarning(e, "Error while sending {Id} - skipping for now", email.Id);
                    }
                }
            }
        }

        private async Task SendEmailUsingNotifyAsync(Email email, CancellationToken ct)
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
                EmailResponse response = await this.notifyApi.SendEmailAsync(emailRequest, ct);
                email.NotificationId = response.Id;
                email.EmailStatusCode = EmailStatus.Processed;
                email.SentDateTime = DateTime.UtcNow;
                await this.emailDelegate.UpdateEmailAsync(email, ct);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Unexpected error while communicating to GC Notify API for email {Id}", email.Id);
                email.LastRetryDateTime = DateTime.UtcNow;
                email.EmailStatusCode = email.Attempts < this.maxRetries ? EmailStatus.Pending : EmailStatus.Error;
                await this.emailDelegate.UpdateEmailAsync(email, ct);
                throw;
            }
        }

        private async Task SendEmailAsync(Email email, CancellationToken ct)
        {
            Exception? caught = null;
            email.Attempts++;
            try
            {
                using SmtpClient smtpClient = new();
                try
                {
                    await smtpClient.ConnectAsync(this.host, this.port, SecureSocketOptions.None, ct);
                    try
                    {
                        using MimeMessage message = PrepareMessage(email);
                        await smtpClient.SendAsync(message, ct);
                        email.SmtpStatusCode = (int)SmtpStatusCode.Ok;
                        email.EmailStatusCode = EmailStatus.Processed;
                        email.SentDateTime = DateTime.UtcNow;
                        await this.emailDelegate.UpdateEmailAsync(email, ct);
                    }
                    catch (SmtpCommandException e)
                    {
                        caught = e;
                        this.logger.LogError(e, "Unexpected error while sending email {Id}, SMTP Error = {SmtpStatusCode}", email.Id, email.SmtpStatusCode);
                    }

                    await smtpClient.DisconnectAsync(true, ct);
                }
                catch (SmtpCommandException e)
                {
                    caught = e;
                    this.logger.LogError(
                        e,
                        "Unexpected error while connecting to SMTP Server to send email {Id}, SMTP Error = {SmtpStatusCode}",
                        email.Id.ToString(),
                        email.SmtpStatusCode);
                }
            }
            catch (Exception e)
            {
                caught = e;
                this.logger.LogError(e, "Unexpected error while sending email {Id}", email.Id);
            }

            if (caught != null)
            {
                email.LastRetryDateTime = DateTime.UtcNow;
                email.EmailStatusCode = email.Attempts < this.maxRetries ? EmailStatus.Pending : EmailStatus.Error;

                if (caught is SmtpCommandException smtpCommandException)
                {
                    email.SmtpStatusCode = (int)smtpCommandException.StatusCode;
                }

                await this.emailDelegate.UpdateEmailAsync(email, ct);

                // Rethrow our exception, Hangfire will catch it and re-schedule the job.
                throw caught;
            }
        }
    }
}
