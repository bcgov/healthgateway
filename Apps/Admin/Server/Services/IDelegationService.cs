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
namespace HealthGateway.Admin.Server.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// The Delegation Service.
    /// </summary>
    public interface IDelegationService
    {
        /// <summary>
        /// Retrieves delegation information for a person.
        /// </summary>
        /// <param name="phn">The phn to query on.</param>
        /// <returns>Information about the person and their delegates.</returns>
        Task<DelegationInfo> GetDelegationInformationAsync(string phn);

        /// <summary>
        /// Protects the dependent and if necessary creates the allowed delegation(s) and keeps the resource delegates
        /// synchronized.
        /// </summary>
        /// <param name="dependentHdid">The hdid of the dependent to protect.</param>
        /// <param name="delegateHdids">The list of delegate hdid(s) to allow delegation for the dependent.</param>
        /// <returns>The task.</returns>
        Task ProtectDependentAsync(string dependentHdid, IEnumerable<string> delegateHdids);

        /// <summary>
        /// Unprotects the dependent and if necessary creates the allowed delegation(s) and keeps the resource delegates
        /// synchronized.
        /// </summary>
        /// <param name="dependentHdid">The hdid of the dependent to unprotect.</param>
        /// <returns>The dependent view.</returns>
        Task UnprotectDependentAsync(string dependentHdid);
    }
}
