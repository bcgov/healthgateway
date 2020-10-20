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
    using System.Collections.Generic;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Models;

    /// <summary>
    /// The Dependent service.
    /// </summary>
    public interface IDependentService
    {
        /// <summary>
        /// Gets all the dependents for the given hdId.
        /// </summary>
        /// <param name="hdId">The users HDID.</param>
        /// <param name="page">The page of data to fetch indexed from 0.</param>
        /// <param name="pageSize">The amount of records per page.</param>
        /// <returns>A List of dependents wrapped in a RequestResult.</returns>
        RequestResult<IEnumerable<DependentModel>> GetDependents(string hdId, int page = 0, int pageSize = 500);

        /// <summary>
        /// Add a dependents to the given hdId of the delegate (parent or guardiant).
        /// </summary>
        /// <param name="delegateHdId">The HdId of the Delegate (parent or guardian).</param>
        /// <param name="addDependentRequest">The request to create a User Delegate model.</param>
        /// <returns>A dependent model wrapped in a RequestResult.</returns>
        RequestResult<DependentModel> AddDependent(string delegateHdId, AddDependentRequest addDependentRequest);
    }
}
