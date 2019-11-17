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
    using HealthGateway.Database.Models;

    /// <summary>
    /// Interface for sending email.
    /// </summary>
    public interface IEmailDelegate
    {
        /// <summary>
        /// Fetches the email object from the database;
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns></returns>
        Email GetEmail(Guid emailId);

        /// <summary>
        /// Inserts an email using a populated Email object.
        /// </summary>
        /// <param name="email">The populated email to save.</param>
        /// <returns>Returns the guid of the saved email.</returns>
        Guid InsertEmail(Email email);

        /// <summary>
        /// Updates an email using a populated Email object.
        /// </summary>
        /// <param name="email">The populated email to save.</param>
        /// <returns>Returns the guid of the saved email.</returns>
        void UpdateEmail(Email email);

        /// <summary>
        /// Looks up an Email Template in the database.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        /// <returns>The populated Email template or null if not found.</returns>
        EmailTemplate GetEmailTemplate(string templateName);
    }
}
