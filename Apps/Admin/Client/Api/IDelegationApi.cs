//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Admin.Client.Api
{
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;
    using Refit;

    /// <summary>
    /// API for interacting with delegation.
    /// </summary>
    public interface IDelegationApi
    {
        /// <summary>
        /// Retrieves delegation information for a person.
        /// </summary>
        /// <param name="phn">The phn to query on.</param>
        /// <returns>Information about the person and their delegates.</returns>
        [Get("/")]
        Task<DelegationInfo> GetDelegationInformationAsync([Header("phn")] string phn);

        /// <summary>
        /// Retrieves information about a potential delegate.
        /// </summary>
        /// <param name="phn">The phn to query on.</param>
        /// <returns>Information about the potential delegate.</returns>
        [Get("/Delegate")]
        Task<DelegateInfo> GetDelegateInformationAsync([Header("phn")] string phn);

        /// <summary>
        /// Protects the dependent and if necessary creates the allowed delegation(s) and keeps the resource delegates
        /// synchronized.
        /// </summary>
        /// <param name="dependentHdid">The hdid of the dependent to protect.</param>
        /// <param name="request">The request object containing data used to protect a dependent.</param>
        /// <returns>The agent action entry created from the operation.</returns>
        [Put("/{dependentHdid}/ProtectDependent")]
        Task<AgentAction> ProtectDependentAsync(string dependentHdid, ProtectDependentRequest request);

        /// <summary>
        /// Unprotects the dependent and if necessary removes the allowed delegation(s) and keeps the resource delegates
        /// synchronized.
        /// </summary>
        /// <param name="dependentHdid">The hdid of the dependent to unprotect.</param>
        /// <param name="request">The request object containing data used to unprotect a dependent.</param>
        /// <returns>The agent action entry created from the operation.</returns>
        [Put("/{dependentHdid}/UnprotectDependent")]
        Task<AgentAction> UnprotectDependentAsync(string dependentHdid, UnprotectDependentRequest request);
    }
}
