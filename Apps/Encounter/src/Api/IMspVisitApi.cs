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
namespace HealthGateway.Encounter.Api
{
    using System.Threading.Tasks;
    using HealthGateway.Encounter.Models.ODR;
    using Refit;

    /// <summary>
    /// API for all Msp Visits for the current user.
    /// </summary>
    public interface IMspVisitApi
    {
        /// <summary>
        /// Returns a list of msp visits.
        /// </summary>
        /// <param name="request">The Encounter request to execute against ODR.</param>
        /// <returns>The Encounter Model response wrapped in an Api Response.</returns>
        [Post("/odr/mspVisits")]
        Task<MspVisitHistory> GetMspVisitsAsync(MspVisitHistory request);
    }
}
