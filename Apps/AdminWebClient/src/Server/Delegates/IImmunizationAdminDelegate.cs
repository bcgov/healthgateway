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
namespace HealthGateway.Admin.Server.Delegates
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Models.Immunization;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// Provides access to Administrative Immunization data.
    /// </summary>
    public interface IImmunizationAdminDelegate
    {
        /// <summary>
        /// Gets a specific immunization by id.
        /// </summary>
        /// <param name="immunizationId">The Immunization to fetch.</param>
        /// <returns>The wrapped Immunization response.</returns>
        Task<RequestResult<Common.Models.Immunization.ImmunizationEvent>> GetImmunizationEvent(string immunizationId);

        /// <summary>
        /// Gets a specific immunization by id.
        /// </summary>
        /// <param name="immunizationId">The Immunization to fetch.</param>
        /// <returns>The wrapped Immunization response.</returns>
        Task<RequestResult<PHSAResult<ImmunizationViewResponse>>> GetImmunization(string immunizationId);

        /// <summary>
        /// Gets the immunizations for the provided Patient.
        /// The patient must have the PHN and DOB provided.
        /// </summary>
        /// <param name="patient">The patient to query for Immunizations.</param>
        /// <param name="pageIndex">The page index to return.</param>
        /// <returns>The wrapped Immunization response.</returns>
        Task<RequestResult<PHSAResult<IList<ImmunizationViewResponse>>>> GetImmunizations(PatientModel patient, int pageIndex = 0);

        /// <summary>
        /// Gets the immunizations for the provided Patient.
        /// The patient must have the PHN and DOB provided.
        /// </summary>
        /// <param name="patient">The patient to query for Immunizations.</param>
        /// <param name="pageIndex">The page index to return.</param>
        /// <returns>The wrapped Immunization response.</returns>
        Task<RequestResult<ImmunizationResult>> GetImmunizationEvents(PatientModel patient, int pageIndex = 0);
    }
}
