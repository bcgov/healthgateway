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
namespace HealthGateway.GatewayApi.Services
{
    using System;
    using HealthGateway.Common.Data.ViewModels;

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
        /// <returns>Returns a request result with appropriate result status..</returns>
        RequestResult<bool> ValidateEmail(string hdid, Guid inviteKey);

        /// <summary>
        /// Creates the a non-validated email.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="emailAddress">Email address to be set for the user.</param>
        /// <param name="isVerified">Indicates whether the email address is verified.</param>
        /// <returns>returns true if the email was sucessfully created.</returns>
        bool CreateUserEmail(string hdid, string emailAddress, bool isVerified);

        /// <summary>
        /// Updates the user email.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="emailAddress">Email address to be set for the user.</param>
        /// <returns>returns true if the email was sucessfully created.</returns>
        bool UpdateUserEmail(string hdid, string emailAddress);
    }
}
