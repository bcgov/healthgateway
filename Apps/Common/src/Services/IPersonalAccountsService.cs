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
namespace HealthGateway.Common.Services
{
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// Caching Personal Account Service.
    /// </summary>
    public interface IPersonalAccountsService
    {
        /// <summary>
        /// Gets personal account information from PHSA using the supplied HDID.
        /// </summary>
        /// <param name="hdid">The HDID to look up.</param>
        /// <returns>The personal account model.</returns>
        Task<PersonalAccount> GetPatientAccountAsync(string hdid);

        /// <summary>
        /// Gets personal account information from PHSA using the supplied HDID.
        /// </summary>
        /// <param name="hdid">The HDID to look up.</param>
        /// <returns>The personal account model wrapped in a RequestResult.</returns>
        Task<RequestResult<PersonalAccount>> GetPatientAccountResultAsync(string hdid);
    }
}
