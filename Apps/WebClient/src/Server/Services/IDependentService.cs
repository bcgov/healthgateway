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
    using System.Collections.Generic;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Models;

    /// <summary>
    /// The Dependent service.
    /// </summary>
    public interface IDependentService
    {
        /// <summary>
        /// Saves the User Delegate to the database.
        /// </summary>
        /// <param name="delegateHdId">The HdId of the Delegate (parent or guardian).</param>
        /// <param name="registerDependentRequest">The request to create a User Delegate model.</param>
        /// <returns>A dependent model wrapped in a RequestResult.</returns>
        RequestResult<DependentModel> AddDependent(string delegateHdId, AddDependentRequest registerDependentRequest);
    }
}
