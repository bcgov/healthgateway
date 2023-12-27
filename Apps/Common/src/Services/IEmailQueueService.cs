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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Provides a mechanism to store an email to the dabase and queue it for delivery.
    /// The batch scheduler will pickup the queued email and process it.
    /// </summary>
    public interface IEmailQueueService
    {
        /// <summary>
        /// Queues a new email based on a template name.
        /// Template will be looked up in the DB.
        /// A new email will be added to the database.
        /// </summary>
        /// <param name="toEmail">The To email address.</param>
        /// <param name="templateName">The template to search the database for.</param>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task QueueNewEmailAsync(string toEmail, string templateName, bool shouldCommit = true, CancellationToken ct = default);

        /// <summary>
        /// Queues a new email based on a template name.
        /// Template will be looked up in the DB.
        /// A new email will be added to the database.
        /// </summary>
        /// <param name="toEmail">The To email address.</param>
        /// <param name="templateName">The template to search the database for.</param>
        /// <param name="keyValues">A dictionary of key/value pairs for replacement.</param>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task QueueNewEmailAsync(string toEmail, string templateName, Dictionary<string, string> keyValues, bool shouldCommit = true, CancellationToken ct = default);

        /// <summary>
        /// Queues an email using a resolved template.
        /// A new email will be added to the database.
        /// </summary>
        /// <param name="toEmail">The To email address.</param>
        /// <param name="emailTemplate">The resolved Email Template.</param>
        /// <param name="keyValues">A dictionary of key/value pairs for replacement.</param>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task QueueNewEmailAsync(string toEmail, EmailTemplate emailTemplate, Dictionary<string, string> keyValues, bool shouldCommit = true, CancellationToken ct = default);

        /// <summary>
        /// Queues an email using a populated Email object.
        /// A new email will be added to the database.
        /// </summary>
        /// <param name="email">The populated email to save.</param>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task QueueNewEmailAsync(Email email, bool shouldCommit = true, CancellationToken ct = default);

        /// <summary>
        /// Looks up an Email Template in the database.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The populated Email template or null if not found.</returns>
        Task<EmailTemplate?> GetEmailTemplateAsync(string templateName, CancellationToken ct = default);

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
