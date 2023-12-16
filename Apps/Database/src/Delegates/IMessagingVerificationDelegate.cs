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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Delegate that performs operations for the MessageVerification model.
    /// </summary>
    public interface IMessagingVerificationDelegate
    {
        /// <summary>
        /// Inserts a Message Verification and any associated records.
        /// </summary>
        /// <param name="messageVerification">The message verification to insert.</param>
        /// <param name="commit">If set to true the database changes will be persisted immediately.</param>
        /// <param name="ct">A Cancellation token.</param>
        /// <returns>Returns the guid of the saved message verification.</returns>
        Task<Guid> InsertAsync(MessagingVerification messageVerification, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Gets the last Email Message Verification by the Invite key.
        /// </summary>
        /// <param name="inviteKey">The users inviteKey as emailed.</param>
        /// <returns>The message verification that was fetched.</returns>
        MessagingVerification? GetLastByInviteKey(Guid inviteKey);

        /// <summary>
        /// Gets the last Email Message Verification by the Invite key.
        /// </summary>
        /// <param name="inviteKey">The users inviteKey as emailed.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The message verification that was fetched.</returns>
        Task<MessagingVerification?> GetLastByInviteKeyAsync(Guid inviteKey, CancellationToken ct = default);

        /// <summary>
        /// Gets the last Messaging Verification for the user base on type and users HDID.
        /// Defaults to MessagingVerificationType.Email.
        /// </summary>
        /// <param name="hdid">The users hdid.</param>
        /// <param name="messagingVerificationType">The type to query.</param>
        /// <returns>The  message verification that was fetched.</returns>
        MessagingVerification? GetLastForUser(string hdid, string messagingVerificationType);

        /// <summary>
        /// Gets the last Messaging Verification for the user base on type and users HDID.
        /// Defaults to MessagingVerificationType.Email.
        /// </summary>
        /// <param name="hdid">The users hdid.</param>
        /// <param name="messagingVerificationType">The type to query.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The  message verification that was fetched.</returns>
        Task<MessagingVerification?> GetLastForUserAsync(string hdid, string messagingVerificationType, CancellationToken ct = default);

        /// <summary>
        /// Updates a MessagingVerification using a populated model object.
        /// </summary>
        /// <param name="messageVerification">The populated email to save.</param>
        /// <param name="commit">If commit is set to true the change will be persisted immediately.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateAsync(MessagingVerification messageVerification, bool commit = true, CancellationToken ct = default);

        /// <summary>
        /// Gets all email message verifications from the database.
        /// </summary>
        /// <returns>A list of message verifications.</returns>
        IEnumerable<MessagingVerification> GetAllEmail();

        /// <summary>
        /// Expire a MessagingVerification.
        /// </summary>
        /// <param name="messageVerification">The message verification to expire.</param>
        /// <param name="markDeleted">Mark the verification as deleted.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ExpireAsync(MessagingVerification messageVerification, bool markDeleted, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a list of messaging verifications matching the query.
        /// </summary>
        /// <param name="hdid">The HDID associated with the messaging verifications.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>A list of messaging verifications matching the query.</returns>
        Task<IList<MessagingVerification>> GetUserMessageVerificationsAsync(string hdid, CancellationToken ct = default);
    }
}
