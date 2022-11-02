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
namespace HealthGateway.Encounter.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Encounter.Models;
    using HealthGateway.Encounter.Models.PHSA;

    /// <summary>
    /// The Encounter data service.
    /// </summary>
    public interface IEncounterService
    {
        /// <summary>
        /// Gets a list of Encounters.
        /// </summary>
        /// <param name="hdid">The health directed id for the subject.</param>
        /// <returns>Returns a list of claims.</returns>
        Task<RequestResult<IEnumerable<EncounterModel>>> GetEncounters(string hdid);

        /// <summary>
        /// Gets a list of Hospital Visits.
        /// </summary>
        /// <param name="hdid">The health directed id for the subject.</param>
        /// <returns>Returns the hospital visit result.</returns>
        Task<RequestResult<HospitalVisitResult>> GetHospitalVisits(string hdid);
    }
}
