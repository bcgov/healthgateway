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
namespace HealthGateway.Common.Api
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Models.PHSA;
    using Refit;

    /// <summary>
    /// Interface to interact with PHSA Personal Accounts API.
    /// </summary>
    public interface IPersonalAccountsApi
    {
        /// <summary>
        /// Retrieves the Personal Account by HDID.
        /// </summary>
        /// <param name="hdid">The HDID to lookup.</param>
        /// <returns>The Personal Account matching the id.</returns>
        [Get("/personal-accounts/hdid/{hdid}")]
        Task<PersonalAccount> AccountLookupByHdidAsync(string hdid);

        /// <summary>
        /// Retrieves the Personal Account by PID.
        /// </summary>
        /// <param name="pid">The PID to lookup.</param>
        /// <returns>The Personal Account matching the id.</returns>
        [Get("/personal-accounts/pid/{pid}")]
        Task<PersonalAccount> AccountLookupByPidAsync(string pid);

        /// <summary>
        /// Retrieves the Personal Account by Account ID.
        /// </summary>
        /// <param name="accountId">The AccountID to lookup.</param>
        /// <returns>The Personal Account matching the id.</returns>
        [Get("/personal-accounts/accountid/{accountId}")]
        Task<PersonalAccount> AccountLookupByIdAsync(string accountId);
    }
}
