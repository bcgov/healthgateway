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
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class EmailJob : IEmailJob
    {
        private readonly ILogger<EmailJob> logger;
        private readonly IEmailDelegate emailDelegate;
        private readonly ISmtpDelegate smtpDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailJob"/> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="emailDelegate"></param>
        /// <param name="smtpDelegate"></param>
        public EmailJob(ILogger<EmailJob> logger, IEmailDelegate emailDelegate, ISmtpDelegate smtpDelegate)
        {
            this.logger = logger;
            this.emailDelegate = emailDelegate;
            this.smtpDelegate = smtpDelegate;
        }

        /// <inheritdoc />
        public void SendEmail(Guid emailId)
        {
            logger.LogTrace($"Starting send of email {emailId}");
            Email email = this.emailDelegate.GetEmail(emailId);
            if (email != null && email.EmailStatusCode == EmailStatus.New && email.Priority >= EmailPriority.Standard)
            {
                SendEmail(email);
            }
            else
            {
                if (email is null)
                {
                    logger.LogWarning($"Unable to find email with id {emailId}");
                }
                else
                {
                    if (email.EmailStatusCode != EmailStatus.New)
                    {
                        logger.LogWarning($"Email {emailId} is in the state of {email.EmailStatusCode} and will not be sent.");
                    }
                    else
                    {
                        logger.LogInformation($"Email {emailId} is low priority and will be deferred to a later time.");
                    }
                }
            }
        }

        private void SendEmail(Email email)
        {
            Contract.Requires(email != null);
            bool err = false;
            email.Attempts++;
            try
            {
                email.SmtpStatusCode = smtpDelegate.SendEmail(email);
                if (email.SmtpStatusCode == smtpDelegate.Ok)
                {
                    email.SentDateTime = DateTime.UtcNow;
                    email.EmailStatusCode = EmailStatus.Processed;
                }
                else
                {
                    err = true;
                    logger.LogError($"Unexpected error while sending email {email.Id}, SMTP Error = {email.SmtpStatusCode}");
                }  
            }
            catch (Exception e)
            {
                err = true;
                logger.LogError($"Unexpected error while sending email {email.Id}", e);
            }
            if (err)
            {
                email.LastRetryDateTime = DateTime.UtcNow;
                email.EmailStatusCode = email.Attempts < 9 ? EmailStatus.Pending : EmailStatus.Error;
            }
            this.emailDelegate.UpdateEmail(email);
        }
    }
}
