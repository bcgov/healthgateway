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
    /// Delegate that performs operations for the EmailInvite model.
    /// </summary>
    public interface IEmailInviteDelegate
    {
        /// <summary>
        /// Inserts an email invite and the associated email.
        /// </summary>
        /// <param name="invite">The invite to insert.</param>
        /// <returns>Returns the guid of the saved email invite.</returns>
        Guid Insert(EmailInvite invite);

        /// <summary>
        /// Gets a particular EmailInvite based on the users HDID and inviteKey.
        /// </summary>
        /// <param name="inviteKey">The users inviteKey as emailed.</param>
        /// <returns>The EmailInvite that was fetched.</returns>
        EmailInvite GetByInviteKey(Guid inviteKey);

        /// <summary>
        /// Gets the last EmailInvite based on the users HDID.
        /// </summary>
        /// <param name="hdid">The users hdid.</param>
        /// <returns>The EmailInvite that was fetched.</returns>
        EmailInvite GetLastForUser(string hdid);

        /// <summary>
        /// Updates an Email Invite using a populated EmailInvite object.
        /// </summary>
        /// <param name="emailInvite">The populated email to save.</param>
        void Update(EmailInvite emailInvite);
    }
}
