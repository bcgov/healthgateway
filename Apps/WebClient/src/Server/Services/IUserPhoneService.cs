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

    /// <summary>
    /// The User Phone service.
    /// </summary>
    public interface IUserPhoneService
    {
        /// <summary>
        /// Updates the user phone number.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="phone">Phone number to be set for the user.</param>
        /// <param name="hostUri">The host uri for referal purposes.</param>
        /// <param name="bearerToken">The security token representing the authenticated user.</param>
        /// <returns>returns true if the phone number was sucessfully updated.</returns>
        bool UpdateUserPhone(string hdid, string phone, Uri hostUri, string bearerToken);
    }
}
