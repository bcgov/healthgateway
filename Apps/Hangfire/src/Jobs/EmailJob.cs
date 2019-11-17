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
namespace HealthGateway.Hangfire.Jobs
{
    using System;
    using System.Diagnostics.Contracts;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using MailKit.Net.Smtp;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using MimeKit;

    /// <inheritdoc />
    public class EmailJob : IEmailJob
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<EmailJob> logger;
        private readonly IEmailDelegate emailDelegate;
        private readonly string host;
        private readonly int port;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailJob"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="emailDelegate">The email delegate to use.</param>
        public EmailJob(IConfiguration configuration, ILogger<EmailJob> logger, IEmailDelegate emailDelegate)
        {
            Contract.Requires(configuration != null && logger != null && emailDelegate != null);
            this.configuration = configuration;
            this.logger = logger;
            this.emailDelegate = emailDelegate;
            IConfigurationSection section = configuration.GetSection("Smtp");
            this.host = section.GetValue<string>("Host");
            this.port = section.GetValue<int>("Port");
        }

        /// <inheritdoc />
        public void SendEmail(Guid emailId)
        {
            this.logger.LogTrace($"Starting send of email {emailId}");
            Email email = this.emailDelegate.GetNewEmail(emailId);
            if (email != null)
            {
                this.SendEmail(email);
            }
            else
            {
                this.logger.LogInformation($"Email {emailId} was not returned from DB, skipping.");
            }
        }

        private static MimeMessage PrepareMessage(Email email)
        {
            MimeMessage msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("Health Gateway", email.From));
            msg.To.Add(new MailboxAddress(email.To));
            msg.Subject = email.Subject;
            msg.Body = new TextPart(email.FormatCode == EmailFormat.HTML ? MimeKit.Text.TextFormat.Html : MimeKit.Text.TextFormat.Plain)
            {
                Text = email.Body,
            };
            return msg;
        }

        private void SendEmail(Email email)
        {
            Exception caught = null;
            email.Attempts++;
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    try
                    {
                        smtpClient.Connect(this.host, this.port);
                        try
                        {
                            smtpClient.Send(EmailJob.PrepareMessage(email));
                            email.SmtpStatusCode = (int)SmtpStatusCode.Ok;
                            email.EmailStatusCode = EmailStatus.Processed;
                            this.emailDelegate.UpdateEmail(email);
                        }
                        catch (SmtpCommandException e)
                        {
                            caught = e;
                            this.logger.LogError($"Unexpected error while sending email {email.Id}, SMTP Error = {email.SmtpStatusCode}", e);
                        }

                        smtpClient.Disconnect(true);
                    }
                    catch (SmtpCommandException e)
                    {
                        caught = e;
                        this.logger.LogError($"Unexpected error while connecting to SMTP Server to send email {email.Id}, SMTP Error = {email.SmtpStatusCode}", e);
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
                email.EmailStatusCode = email.Attempts < 9 ? EmailStatus.Pending : EmailStatus.Error;
                if (caught is SmtpCommandException)
                {
                    email.SmtpStatusCode = (int)((SmtpCommandException)caught).StatusCode;
                }

                this.emailDelegate.UpdateEmail(email);

                // Rethrow our exception, Hangfire will catch it and re-schedule the job.
                throw caught;
            }
        }
    }
}
