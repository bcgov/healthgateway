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
    using System.Collections.Generic;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Operations to be performed in the DB for the Beta request.
    /// </summary>
    public interface IBetaRequestDelegate
    {
        /// <summary>
        /// Creates a BetaRequest object in the database.
        /// </summary>
        /// <param name="betaRequest">The beta request to create.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<BetaRequest> InsertBetaRequest(BetaRequest betaRequest);

        /// <summary>
        /// Updates the BetaRequest object in the DB.
        /// Version must be set or a Concurrency exception will occur.
        /// UpdatedDateTime will overridden by our framework.
        /// </summary>
        /// <param name="betaRequest">The beta request to update.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<BetaRequest> UpdateBetaRequest(BetaRequest betaRequest);

        /// <summary>
        /// Fetches the BetaRequest from the database.
        /// </summary>
        /// <param name="hdId">The hdid associated with the beta request to find.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<BetaRequest> GetBetaRequest(string hdId);

        /// <summary>
        /// Fetches the pending BetaRequest from the database.
        /// </summary>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DBResult<List<BetaRequest>> GetPendingBetaRequest();
    }
}
