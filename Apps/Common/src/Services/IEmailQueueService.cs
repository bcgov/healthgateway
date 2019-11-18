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
    using HealthGateway.Database.Models;

    /// <summary>
    /// Provides a mechanism to store an email to the dabase and queue it for delivery.
    /// The batch scheduler will pickup the queued email and process it.
    /// </summary>
    public interface IEmailQueueService
    {
        /// <summary>
        /// Queues an email based on a template name.
        /// Template will be looked up in the DB.
        /// </summary>
        /// <param name="toEmail">The To email address.</param>
        /// <param name="templateName">The template to search the database for.</param>
        /// <param name="keyValues">A dictionary of key/value pairs for replacement.</param>
        void QueueEmail(string toEmail, string templateName, Dictionary<string, string> keyValues);

        /// <summary>
        /// Queues an email using a resolved template.
        /// </summary>
        /// <param name="toEmail">The To email address.</param>
        /// <param name="emailTemplate">The resolved Email Template.</param>
        /// <param name="keyValues">A dictionary of key/value pairs for replacement.</param>
        void QueueEmail(string toEmail, EmailTemplate emailTemplate, Dictionary<string, string> keyValues);

        /// <summary>
        /// Queues an email using a populated Email object.
        /// </summary>
        /// <param name="email">The populated email to save.</param>
        void QueueEmail(Email email);

        /// <summary>
        /// Queues an email invite based on a template name.
        /// Template will be looked up in the DB.
        /// </summary>
        /// <param name="hdid">The users hdid to associate the invite with.</param>
        /// <param name="toEmail">The To email address.</param>
        /// <param name="templateName">The template to search the database for.</param>
        /// <param name="keyValues">A dictionary of key/value pairs for replacement.</param>
        void QueueInviteEmail(string hdid, string toEmail, string templateName, Dictionary<string, string> keyValues);

        /// <summary>
        /// Queues an email invite using a resolved template.
        /// </summary>
        /// <param name="hdid">The users hdid to associate the invite with.</param>
        /// <param name="toEmail">The To email address.</param>
        /// <param name="emailTemplate">The resolved Email Template.</param>
        /// <param name="keyValues">A dictionary of key/value pairs for replacement.</param>
        void QueueInviteEmail(string hdid, string toEmail, EmailTemplate emailTemplate, Dictionary<string, string> keyValues);

        /// <summary>
        /// Queues an email using a populated Email object.
        /// </summary>
        /// <param name="invite">The populated invite with associated email to send.</param>
        void QueueInviteEmail(EmailInvite invite);

        /// <summary>
        /// Looks up an Email Template in the database.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        /// <returns>The populated Email template or null if not found.</returns>
        EmailTemplate GetEmailTemplate(string templateName);

        /// <summary>
        /// Given an Email template it will swap the dictionary key/values in the Subject and Body.
        /// </summary>
        /// <param name="toEmail">The To email address.</param>
        /// <param name="emailTemplate">An Email template object.</param>
        /// <param name="keyValues">A dictionary of key/value pairs for replacement.</param>
        /// <returns>The populated email object.</returns>
        Email ProcessTemplate(string toEmail, EmailTemplate emailTemplate, Dictionary<string, string> keyValues);
    }
}
