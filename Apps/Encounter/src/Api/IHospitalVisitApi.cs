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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Encounter.Models.PHSA;
    using Refit;

    /// <summary>
    /// API for all Hospital Visits for the current user.
    /// </summary>
    public interface IHospitalVisitApi
    {
        /// <summary>
        /// Returns a list of hospital visits.
        /// </summary>
        /// <param name="subjectHdid">The Hdid to query hospital visits.</param>
        /// <param name="limit">The Limit to query hospital visits.</param>
        /// <param name="token">The bearer token to authorize the call.</param>
        /// <returns>
        /// The PHSA Result including the load state and the list of hospital visits for the user identified by
        /// the subject id in the query.
        /// </returns>
        [Get("/api/v1/HospitalVisits?subjectHdid={subjectHdid}&limit={limit}")]
        Task<PhsaResult<IEnumerable<HospitalVisit>>> GetHospitalVisitsAsync(string subjectHdid, string limit, [Authorize] string token);
    }
}
