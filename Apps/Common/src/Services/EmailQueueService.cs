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
namespace HealthGateway.Common.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A simple service to queue and send email.
    /// </summary>
    public class EmailQueueService : IEmailQueueService
    {
        private readonly IEmailDelegate emailDelegate;
        private readonly IWebHostEnvironment environment;
        private readonly IBackgroundJobClient jobClient;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailQueueService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="jobClient">The JobScheduler queue client.</param>
        /// <param name="emailDelegate">Email delegate to be used.</param>
        /// <param name="environment">The injected environment configuration.</param>
        public EmailQueueService(
            ILogger<EmailQueueService> logger,
            IBackgroundJobClient jobClient,
            IEmailDelegate emailDelegate,
            IWebHostEnvironment environment)
        {
            this.logger = logger;
            this.jobClient = jobClient;
            this.emailDelegate = emailDelegate;
            this.environment = environment;
        }

        /// <inheritdoc/>
        public void QueueNewEmail(string toEmail, string templateName, bool shouldCommit = true)
        {
            Dictionary<string, string> keyValues = new();
            this.QueueNewEmail(toEmail, templateName, keyValues, shouldCommit);
        }

        /// <inheritdoc/>
        public async Task QueueNewEmailAsync(string toEmail, string templateName, bool shouldCommit = true, CancellationToken ct = default)
        {
            Dictionary<string, string> keyValues = new();
            await this.QueueNewEmailAsync(toEmail, templateName, keyValues, shouldCommit, ct);
        }

        /// <inheritdoc/>
        public void QueueNewEmail(string toEmail, string templateName, Dictionary<string, string> keyValues, bool shouldCommit = true)
        {
            this.QueueNewEmail(toEmail, this.GetEmailTemplate(templateName), keyValues, shouldCommit);
        }

        /// <inheritdoc/>
        public async Task QueueNewEmailAsync(string toEmail, string templateName, Dictionary<string, string> keyValues, bool shouldCommit = true, CancellationToken ct = default)
        {
            EmailTemplate? emailTemplate = await this.GetEmailTemplateAsync(templateName, ct) ?? throw new ProblemDetailsException(
                ExceptionUtility.CreateServerError($"{ServiceType.Database}:{ErrorType.CommunicationInternal}", ErrorMessages.EmailTemplateNotFound));
            await this.QueueNewEmailAsync(toEmail, emailTemplate, keyValues, shouldCommit, ct);
        }

        /// <inheritdoc/>
        public void QueueNewEmail(string toEmail, EmailTemplate emailTemplate, Dictionary<string, string> keyValues, bool shouldCommit = true)
        {
            this.QueueNewEmail(this.ProcessTemplate(toEmail, emailTemplate, keyValues), shouldCommit);
        }

        /// <inheritdoc/>
        public async Task QueueNewEmailAsync(string toEmail, EmailTemplate emailTemplate, Dictionary<string, string> keyValues, bool shouldCommit = true, CancellationToken ct = default)
        {
            await this.QueueNewEmailAsync(this.ProcessTemplate(toEmail, emailTemplate, keyValues), shouldCommit, ct);
        }

        /// <inheritdoc/>
        public void QueueNewEmail(Email email, bool shouldCommit = true)
        {
            if (string.IsNullOrWhiteSpace(email.To))
            {
                throw new ArgumentNullException(nameof(email), "Email To cannot be null or whitespace");
            }

            this.logger.LogTrace("Queueing email...");
            if (email.Id == Guid.Empty)
            {
                this.emailDelegate.InsertEmail(email, shouldCommit);
            }

            if (shouldCommit)
            {
                this.jobClient.Enqueue<IEmailJob>(j => j.SendEmail(email.Id));
            }

            this.logger.LogDebug("Finished queueing email. {Id}", email.Id);
        }

        /// <inheritdoc/>
        public async Task QueueNewEmailAsync(Email email, bool shouldCommit = true, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email.To))
            {
                throw new ArgumentNullException(nameof(email), "Email To cannot be null or whitespace");
            }

            this.logger.LogTrace("Queueing email...");
            if (email.Id == Guid.Empty)
            {
                await this.emailDelegate.InsertEmailAsync(email, shouldCommit, ct);
            }

            if (shouldCommit)
            {
                this.jobClient.Enqueue<IEmailJob>(j => j.SendEmail(email.Id));
            }

            this.logger.LogDebug("Finished queueing email. {Id}", email.Id);
        }

        /// <inheritdoc/>
        public void CloneAndQueue(Guid emailId, bool shouldCommit = true)
        {
            Email? oldEmail = this.emailDelegate.GetEmail(emailId);
            if (oldEmail != null)
            {
                Email email = new()
                {
                    From = oldEmail.From,
                    To = oldEmail.To,
                    Subject = oldEmail.Subject,
                    Body = oldEmail.Body,
                    FormatCode = oldEmail.FormatCode,
                    Priority = oldEmail.Priority,
                    Personalization = oldEmail.Personalization,
                    Template = oldEmail.Template,
                };
                this.QueueNewEmail(email, shouldCommit);
            }
            else
            {
                throw new ArgumentException($"emailID: {emailId} was not found in the DB", nameof(emailId));
            }
        }

        /// <inheritdoc/>
        public EmailTemplate GetEmailTemplate(string templateName)
        {
            this.logger.LogTrace("Getting email template... {TemplateName}", templateName);
            EmailTemplate retVal = this.emailDelegate.GetEmailTemplate(templateName);
            this.logger.LogDebug("Finished getting email template");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<EmailTemplate?> GetEmailTemplateAsync(string templateName, CancellationToken ct = default)
        {
            this.logger.LogTrace("Getting email template... {TemplateName}", templateName);
            EmailTemplate? retVal = await this.emailDelegate.GetEmailTemplateAsync(templateName, ct);
            this.logger.LogDebug("Finished getting email template");
            return retVal;
        }

        /// <inheritdoc/>
        public Email ProcessTemplate(string toEmail, string templateName, Dictionary<string, string> keyValues)
        {
            return this.ProcessTemplate(toEmail, this.GetEmailTemplate(templateName), keyValues);
        }

        /// <inheritdoc/>
        public Email ProcessTemplate(string toEmail, EmailTemplate emailTemplate, Dictionary<string, string> keyValues)
        {
            this.logger.LogTrace("Processing template... {Name}", emailTemplate.Name);
            Email email = this.ParseTemplate(emailTemplate, keyValues);
            email.To = toEmail;
            this.logger.LogDebug("Finished processing template");
            return email;
        }

        private Email ParseTemplate(EmailTemplate emailTemplate, Dictionary<string, string> keyValues)
        {
            if (!keyValues.ContainsKey(EmailTemplateVariable.Environment))
            {
                keyValues.Add(EmailTemplateVariable.Environment, this.environment.IsProduction() ? string.Empty : this.environment.EnvironmentName);
            }

            return new Email
            {
                From = emailTemplate.From,
                Priority = emailTemplate.Priority,
                Subject = StringManipulator.Replace(emailTemplate.Subject!, keyValues),
                Body = StringManipulator.Replace(emailTemplate.Body!, keyValues),
                FormatCode = emailTemplate.FormatCode,
                Personalization = keyValues,
                Template = emailTemplate.Name,
            };
        }
    }
}
