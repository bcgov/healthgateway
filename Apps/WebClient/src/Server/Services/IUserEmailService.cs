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
namespace HealthGateway.WebClient.Services
{
    using System;
    using HealthGateway.Database.Models;

    /// <summary>
    /// The User Email service.
    /// </summary>
    public interface IUserEmailService
    {
        /// <summary>
        /// Validates the email that matches the given invite key.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="inviteKey">The email invite key.</param>
        /// <returns>returns true if the email invite was found.</returns>
        bool ValidateEmail(string hdid, Guid inviteKey);

        /// <summary>
        /// Retrieves the last invite email.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <returns>returns the last email invite if found.</returns>
        MessagingVerification RetrieveLastInvite(string hdid);

        /// <summary>
        /// Updates the user email.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="email">Email to be set for the user.</param>
        /// <param name="hostUri">The host uri for referal purposes.</param>
        /// <returns>returns true if the email invite was sucessfully created.</returns>
        bool UpdateUserEmail(string hdid, string email, Uri hostUri);
    }
}
