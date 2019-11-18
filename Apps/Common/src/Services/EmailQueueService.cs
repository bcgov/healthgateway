﻿// -------------------------------------------------------------------------
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
    using System.Diagnostics.Contracts;
    using System.Net.Mail;
    using System.Text.RegularExpressions;
    using Hangfire;
    using HealthGateway.Common.Jobs;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A simple service to queue and send email.
    /// </summary>
    public class EmailQueueService : IEmailQueueService
    {
        private readonly IEmailDelegate emailDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailQueueService"/> class.
        /// </summary>
        /// <param name="emailDelegate">The delegate to be used.</param>
        public EmailQueueService(IEmailDelegate emailDelegate)
        {
            this.emailDelegate = emailDelegate;
        }

        /// <inheritdoc />
        public void QueueEmail(string toEmail, string templateName, Dictionary<string, string> keyValues)
        {
            this.QueueEmail(toEmail, this.GetEmailTemplate(templateName), keyValues);
        }

        /// <inheritdoc />
        public void QueueEmail(string toEmail, EmailTemplate emailTemplate, Dictionary<string, string> keyValues)
        {
            this.QueueEmail(this.ProcessTemplate(toEmail, emailTemplate, keyValues));
        }

        /// <inheritdoc />
        public void QueueEmail(Email email)
        {
            this.emailDelegate.InsertEmail(email);
            BackgroundJob.Enqueue<IEmailJob>(j => j.SendEmail(email.Id));
        }

        /// <inheritdoc />
        public void QueueInviteEmail(string hdid, string toEmail, string templateName, Dictionary<string, string> keyValues)
        {
            this.QueueInviteEmail(hdid, toEmail, this.GetEmailTemplate(templateName), keyValues);
        }

        /// <inheritdoc />
        public void QueueInviteEmail(string hdid, string toEmail, EmailTemplate emailTemplate, Dictionary<string, string> keyValues)
        {
            EmailInvite invite = new EmailInvite();
            invite.HdId = hdid;
            invite.Email = this.ProcessTemplate(toEmail, emailTemplate, keyValues);
            this.QueueInviteEmail(invite);
        }

        /// <inheritdoc />
        public void QueueInviteEmail(EmailInvite invite)
        {
            this.emailDelegate.InsertEmailInvite(invite);
            BackgroundJob.Enqueue<IEmailJob>(j => j.SendEmail(invite.Email.Id));
        }

        /// <inheritdoc />
        public EmailTemplate GetEmailTemplate(string templateName)
        {
            return this.emailDelegate.GetEmailTemplate(templateName);
        }

        /// <inheritdoc />
        public Email ProcessTemplate(string toEmail, EmailTemplate emailTemplate, Dictionary<string, string> keyValues)
        {
            Email email = ParseTemplate(emailTemplate, keyValues);
            email.To = toEmail;
            return email;
        }

        private static Email ParseTemplate(EmailTemplate emailTemplate, Dictionary<string, string> keyValues)
        {
            Contract.Requires(emailTemplate != null);
            Email email = new Email();
            email.From = emailTemplate.From;
            email.Priority = emailTemplate.Priority;
            email.Subject = ProcessTemplateString(emailTemplate.Subject, keyValues);
            email.Body = ProcessTemplateString(emailTemplate.Body, keyValues);
            email.FormatCode = emailTemplate.FormatCode;
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
               m.Groups.Count > 1 && data.ContainsKey(m.Groups[1].Value) ?
               data[m.Groups[1].Value] : m.Value);
        }
    }
}
