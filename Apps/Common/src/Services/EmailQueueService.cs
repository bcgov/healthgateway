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
    using System.Text.RegularExpressions;
    using Hangfire;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// A simple service to queue and send email.
    /// </summary>
    public class EmailQueueService : IEmailQueueService
    {
#pragma warning disable SA1310 // Disable _ in variable name
        private const string INVITE_KEY_VARIABLE = "InviteKey";
        private const string ACTIVATION_HOST_VARIABLE = "ActivationHost";
        private const string ENVIRONMENT_VARIABLE = "Environment";
#pragma warning restore SA1310 // Restore warnings
        private readonly IEmailDelegate emailDelegate;
        private readonly IEmailInviteDelegate emailInviteDelegate;
        private readonly IWebHostEnvironment enviroment;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailQueueService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="emailDelegate">Email delegate to be used.</param>
        /// <param name="emailInviteDelegate">Invite email delegate to be used.</param>
        /// <param name="enviroment">The injected environment configuration.</param>
        public EmailQueueService(
            ILogger<EmailQueueService> logger,
            IEmailDelegate emailDelegate,
            IEmailInviteDelegate emailInviteDelegate,
            IWebHostEnvironment enviroment)
        {
            this.logger = logger;
            this.emailDelegate = emailDelegate;
            this.emailInviteDelegate = emailInviteDelegate;
            this.enviroment = enviroment;
        }

        /// <inheritdoc />
        public void QueueNewEmail(string toEmail, string templateName)
        {
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            this.QueueNewEmail(toEmail, templateName, keyValues);
        }

        /// <inheritdoc />
        public void QueueNewEmail(string toEmail, string templateName, Dictionary<string, string> keyValues)
        {
            this.QueueNewEmail(toEmail, this.GetEmailTemplate(templateName), keyValues);
        }

        /// <inheritdoc />
        public void QueueNewEmail(string toEmail, EmailTemplate emailTemplate, Dictionary<string, string> keyValues)
        {
            this.QueueNewEmail(this.ProcessTemplate(toEmail, emailTemplate, keyValues));
        }

        /// <inheritdoc />
        public void QueueNewEmail(Email email)
        {
            this.logger.LogTrace($"Queueing email... {JsonConvert.SerializeObject(email)}");
            this.emailDelegate.InsertEmail(email);
            BackgroundJob.Enqueue<IEmailJob>(j => j.SendEmail(email.Id));
            this.logger.LogDebug($"Finished queueing email. {email.Id}");
        }

        /// <inheritdoc />
        public void QueueNewInviteEmail(string hdid, string toEmail, Uri activationHost)
        {
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            EmailInvite invite = new EmailInvite();
            invite.InviteKey = Guid.NewGuid();
            invite.HdId = hdid;
            invite.ExpireDate = DateTime.MaxValue;

            string hostUrl = activationHost.ToString();
            hostUrl = hostUrl.Remove(hostUrl.Length - 1, 1); // Strips last slash

            keyValues.Add(INVITE_KEY_VARIABLE, invite.InviteKey.ToString());
            keyValues.Add(ACTIVATION_HOST_VARIABLE, hostUrl);

            invite.Email = this.ProcessTemplate(toEmail, this.GetEmailTemplate(EmailTemplateName.REGISTRATION_TEMPLATE), keyValues);

            this.QueueNewInviteEmail(invite);
        }

        /// <inheritdoc />
        public void QueueNewInviteEmail(EmailInvite invite)
        {
            this.logger.LogTrace($"Queueing new invite email... {JsonConvert.SerializeObject(invite)}");
            this.emailInviteDelegate.Insert(invite);
            BackgroundJob.Enqueue<IEmailJob>(j => j.SendEmail(invite.Email.Id));
            this.logger.LogDebug($"Finished queueing new invite email. {invite.Id}");
        }

        /// <inheritdoc />
        public void QueueInviteEmail(Guid inviteEmailId)
        {
            this.logger.LogTrace($"Queueing invite email... {JsonConvert.SerializeObject(inviteEmailId)}");
            BackgroundJob.Enqueue<IEmailJob>(j => j.SendEmail(inviteEmailId));
            this.logger.LogDebug($"Finished queueing invite email. {inviteEmailId}");
        }

        /// <inheritdoc />
        public Email ProcessTemplate(string toEmail, EmailTemplate emailTemplate, Dictionary<string, string> keyValues)
        {
            this.logger.LogTrace($"Processing template... {emailTemplate.Name}");
            Email email = this.ParseTemplate(emailTemplate, keyValues);
            email.To = toEmail;
            this.logger.LogDebug($"Finished processing template. {JsonConvert.SerializeObject(email)}");
            return email;
        }

        /// <summary>
        /// A string to scan for keys marked up as ${KEYNAME} to replace.
        /// The dictionary should only have the name of the key as in KEY and NOT ${KEY}.
        /// </summary>
        /// <param name="template">The string to scan and replace.</param>
        /// <param name="data">The dictionary of key/value pairs.</param>
        /// <returns>The string with the key replaced by the supplied values.</returns>
        private static string ProcessTemplateString(string template, Dictionary<string, string> data)
        {
            // The regex will find all instances of ${ANYTHING} and will evaluate if the keys between
            // the mustaches match one of those in the dictionary.  If so it then replaces the match
            // with the value in the dictionary.
            return Regex.Replace(template, "\\$\\{(.*?)\\}", m =>
               (m.Groups.Count > 1 && data.ContainsKey(m.Groups[1].Value)) ?
               data[m.Groups[1].Value] : m.Value);
        }

        /// <summary>
        /// Looks up an Email Template in the database.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        /// <returns>The populated Email template or null if not found.</returns>
        private EmailTemplate GetEmailTemplate(string templateName)
        {
            this.logger.LogTrace($"Getting email template... {templateName}");
            EmailTemplate retVal = this.emailDelegate.GetEmailTemplate(templateName);
            this.logger.LogDebug($"Finished getting email template. {JsonConvert.SerializeObject(retVal)}");
            return retVal;
        }

        private Email ParseTemplate(EmailTemplate emailTemplate, Dictionary<string, string> keyValues)
        {
            if (!keyValues.ContainsKey(ENVIRONMENT_VARIABLE))
            {
                keyValues.Add(ENVIRONMENT_VARIABLE, this.enviroment.IsProduction() ? string.Empty : this.enviroment.EnvironmentName);
            }

            Email email = new Email();
            email.From = emailTemplate.From;
            email.Priority = emailTemplate.Priority;
            email.Subject = ProcessTemplateString(emailTemplate.Subject, keyValues);
            email.Body = ProcessTemplateString(emailTemplate.Body, keyValues);
            email.FormatCode = emailTemplate.FormatCode;
            return email;
        }
    }
}
