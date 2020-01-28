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
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Service that provides functionality to access and create requests for beta access.
    /// </summary>
    public interface IBetaRequestService
    {
        /// <summary>
        /// Retrieves the beta request for a user.
        /// </summary>
        /// <param name="hdid">The requested user hdid.</param>
        /// <returns>returns the beta request for the user if found.</returns>
        BetaRequest GetBetaRequest(string hdid);

        /// <summary>
        /// Saves the user profile to the database.
        /// </summary>
        /// <param name="betaRequest">The request to create a beta request.</param>
        /// <param name="hostUrl">The host url for referal purposes.</param>
        /// <returns>The wrapped user profile.</returns>
        RequestResult<BetaRequest> PutBetaRequest(BetaRequest betaRequest, string hostUrl);
    }
}
