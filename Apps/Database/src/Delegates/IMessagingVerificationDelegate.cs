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
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Delegate that performs operations for the EmailInvite model.
    /// </summary>
    public interface IMessagingVerificationDelegate
    {
        /// <summary>
        /// Inserts a Message Verification and any associated records.
        /// </summary>
        /// <param name="messageVerification">The message verification to insert.</param>
        /// <returns>Returns the guid of the saved message verification.</returns>
        Guid Insert(MessagingVerification messageVerification);

        /// <summary>
        /// Gets an Email Message Verification by the Invite key and the users HDID.
        /// </summary>
        /// <param name="inviteKey">The users inviteKey as emailed.</param>
        /// <returns>The message verification that was fetched.</returns>
        MessagingVerification GetByInviteKey(Guid inviteKey);

        /// <summary>
        /// Gets the last Messaging Verification for the user base on type and users HDID.
        /// Defaults to MessagingVerificationType.Email.
        /// </summary>
        /// <param name="hdid">The users hdid.</param>
        /// <param name="messagingVerificationType">The type to query.</param>
        /// <returns>The  message verification that was fetched.</returns>
        MessagingVerification? GetLastForUser(string hdid, string messagingVerificationType);

        /// <summary>
        /// Updates a MessageingVerification using a populated model object.
        /// </summary>
        /// <param name="messageVerification">The populated email to save.</param>
        void Update(MessagingVerification messageVerification);

        /// <summary>
        /// Gets all email invites from the database.
        /// </summary>
        /// <returns>A list of email invites.</returns>
        IEnumerable<MessagingVerification> GetAllEmail();
    }
}
