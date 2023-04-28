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
namespace HealthGateway.Database.Delegates
{
    using System;
    using System.Collections.Generic;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Interface for sending email.
    /// </summary>
    public interface IEmailDelegate
    {
        /// <summary>
        /// Fetches the email object from the database.
        /// </summary>
        /// <param name="emailId">The Email ID to retrieve.</param>
        /// <returns>The found Email object.</returns>
        Email? GetEmail(Guid emailId);

        /// <summary>
        /// Gets an email by id with standard priority or higher.
        /// </summary>
        /// <param name="emailId">The Email ID to retrieve.</param>
        /// <returns>The found Email object.</returns>
        Email? GetStandardEmail(Guid emailId);

        /// <summary>
        /// Gets a sorted list of mail that needs to be sent.
        /// Low priority for this job is anything less than EmailPriority.Standard
        /// The list will be ordered by the Priority in descending order.
        /// </summary>
        /// <param name="maxRows">The maximum amount of emails to return.</param>
        /// <returns>The list of emails.</returns>
        IList<Email> GetUnsentEmails(int maxRows);

        /// <summary>
        /// Inserts an email using a populated Email object.
        /// </summary>
        /// <param name="email">The populated email to save.</param>
        /// <param name="shouldCommit">If true, the record will be written to the DB immediately.</param>
        /// <returns>Returns the guid of the saved email.</returns>
        Guid InsertEmail(Email email, bool shouldCommit = true);

        /// <summary>
        /// Updates an email using a populated Email object.
        /// </summary>
        /// <param name="email">The populated email to save.</param>
        void UpdateEmail(Email email);

        /// <summary>
        /// Looks up an Email Template in the database.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        /// <returns>The populated Email template or null if not found.</returns>
        EmailTemplate GetEmailTemplate(string templateName);

        /// <summary>
        /// Gets a list of emails ordered by the create datetime descending.
        /// </summary>
        /// <param name="offset">The starting offset for the query.</param>
        /// <param name="pageSize">The maximum amount of rows to return.</param>
        /// <returns>A list of Notes wrapped in a DBResult.</returns>
        DbResult<IList<Email>> GetEmails(int offset = 0, int pageSize = 1000);

        /// <summary>
        /// Deletes email records that were created after n days ago.
        /// </summary>
        /// <param name="daysAgo">Delete emails where created on or before days ago.</param>
        /// <param name="maxRows">The maximum amount of emails to delete at one time.</param>
        /// <param name="shouldCommit">If true, the records will be deleted from the DB immediately.</param>
        /// <returns>The number of rows deleted.</returns>
        int Delete(uint daysAgo, int maxRows, bool shouldCommit = true);
    }
}
