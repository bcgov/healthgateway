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
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Delegate that performs operations for the MessageVerification model.
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
        /// Gets the last Email Message Verification by the Invite key.
        /// </summary>
        /// <param name="inviteKey">The users inviteKey as emailed.</param>
        /// <returns>The message verification that was fetched.</returns>
        MessagingVerification? GetLastByInviteKey(Guid inviteKey);

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
        /// Gets all email message verifications from the database.
        /// </summary>
        /// <returns>A list of message verifications.</returns>
        IEnumerable<MessagingVerification> GetAllEmail();

        /// <summary>
        /// Expire a MessageingVerification.
        /// </summary>
        /// <param name="messageVerification">The message verification to expire.</param>
        /// <param name="markDeleted">Mark the verification as deleted.</param>
        void Expire(MessagingVerification messageVerification, bool markDeleted);

        /// <summary>
        /// Retrieves a list of message verifications matching the query.
        /// </summary>
        /// <param name="queryType">The type of query to perform.</param>
        /// <param name="queryString">The value to query on.</param>
        /// <returns>A list of users matching the query.</returns>
        DBResult<IEnumerable<MessagingVerification>> GetUserMessageVerifications(UserQueryType queryType, string queryString);
    }
}
