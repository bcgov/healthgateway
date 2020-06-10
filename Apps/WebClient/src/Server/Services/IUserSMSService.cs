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
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;

    /// <summary>
    /// The User SMS service.
    /// </summary>
    public interface IUserSMSService
    {
        /// <summary>
        /// Updates the user SMS number.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="sms">SMS number to be set for the user.</param>
        /// <param name="hostUri">The host uri for referal purposes.</param>
        /// <param name="bearerToken">The security token representing the authenticated user.</param>
        /// <returns>returns true if the sms number was sucessfully updated.</returns>
        Task<bool> UpdateUserSMS(string hdid, string sms, Uri hostUri, string bearerToken);

        /// <summary>
        /// Validates the sms number that matches the given validation code.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <param name="validationCode">The sms validation code.</param>
        /// <param name="bearerToken">The security token representing the authenticated user.</param>
        /// <returns>returns true if the sms invite was found and validated.</returns>
        Task<bool> ValidateSMS(string hdid, string validationCode, string bearerToken);

        /// <summary>
        /// Retrieves the last invite SMS.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <returns>returns the last SMS invite if found.</returns>
        MessagingVerification? RetrieveLastInvite(string hdid);
    }
}
